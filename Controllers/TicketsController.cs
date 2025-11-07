using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FixItNR.Api.Controllers
{
    /// <summary>Модель запроса на создание заявки.</summary>
    public record CreateTicketRequest
    {
        /// <summary>Автор заявки (ФИО).</summary>
        [Required]
        public string AuthorName { get; init; } = default!;

        /// <summary>Категория: IT, электрика, уборка (или прочее).</summary>
        [Required]
        public string Category { get; init; } = "IT";

        /// <summary>Место: корпус/этаж/аудитория (например, «Ауд. 207»).</summary>
        [Required]
        public string Place { get; init; } = default!;

        /// <summary>Краткое описание проблемы.</summary>
        [Required]
        public string Description { get; init; } = default!;

        /// <summary>Приоритет: Low / Medium / High.</summary>
        [Required]
        public string Priority { get; init; } = "Medium";
    }

    /// <summary>Заявка (Ticket) с рассчитанным сроком SLA.</summary>
    public record Ticket
    {
        /// <summary>Уникальный идентификатор заявки.</summary>
        public Guid Id { get; init; }

        /// <summary>Автор заявки (ФИО).</summary>
        public string AuthorName { get; init; } = default!;

        /// <summary>Категория обращения.</summary>
        public string Category { get; init; } = default!;

        /// <summary>Место возникновения проблемы.</summary>
        public string Place { get; init; } = default!;

        /// <summary>Текст описания проблемы.</summary>
        public string Description { get; init; } = default!;

        /// <summary>Приоритет обращения.</summary>
        public string Priority { get; init; } = "Medium";

        /// <summary>Время создания (UTC).</summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>Срок исполнения по SLA (UTC).</summary>
        public DateTime SlaDueAt { get; init; }

        /// <summary>Статус заявки (минимальная модель: New).</summary>
        public string Status { get; init; } = "New";
    }

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TicketsController : ControllerBase
    {
        private static readonly List<Ticket> _tickets = new();

        /// <summary>Получить список всех заявок.</summary>
        /// <remarks>Возвращает массив объектов <c>Ticket</c> (может быть пустым).</remarks>
        /// <response code="200">Успешно. Массив Ticket.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Ticket>> Get()
            => Ok(_tickets);

        /// <summary>Получить заявку по идентификатору.</summary>
        /// <param name="id">Идентификатор заявки.</param>
        /// <response code="200">Найдена.</response>
        /// <response code="404">Не найдена.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Ticket> GetById(Guid id)
        {
            var found = _tickets.FirstOrDefault(t => t.Id == id);
            return found is null ? NotFound() : Ok(found);
        }

        /// <summary>Создать новую заявку.</summary>
        /// <remarks>
        /// Рассчитывает поле <c>slaDueAt</c> по категории и приоритету.  
        /// Пример тела:
        /// <code>
        /// {
        ///   "authorName": "Иван",
        ///   "category": "IT",
        ///   "place": "Ауд. 207",
        ///   "description": "Не работает проектор",
        ///   "priority": "High"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="req">Данные новой заявки.</param>
        /// <response code="201">Создано. Возвращает объект Ticket.</response>
        /// <response code="400">Невалидное тело запроса.</response>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Ticket> Post([FromBody] CreateTicketRequest req)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var now = DateTime.UtcNow;

            // Базовые часы по категории
            var category = (req.Category ?? "IT").Trim();
            var baseHours = category.ToLowerInvariant() switch
            {
                "it" => 24,
                "электрика" => 48,
                "уборка" => 36,
                _ => 36
            };

            // Корректировка по приоритету
            var priority = (req.Priority ?? "Medium").Trim();
            var adjust = priority.ToLowerInvariant() switch
            {
                "low" => +12,
                "high" => -12,
                _ => 0
            };

            var hours = Math.Max(6, baseHours + adjust);
            var due = now.AddHours(hours);

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                AuthorName = req.AuthorName,
                Category = req.Category,
                Place = req.Place,
                Description = req.Description,
                Priority = req.Priority,
                CreatedAt = now,
                SlaDueAt = due,
                Status = "New"
            };

            _tickets.Add(ticket);

            // Возвращаем 201 с заголовком Location на GET /api/Tickets/{id}
            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }
    }
}
