using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using FixItNR.Api.Models;   // <-- используем твою модель Ticket с enum’ами
using FixItNR.Api.Data;     // <-- репозиторий из твоего проекта

namespace FixItNR.Api.Controllers
{
    /// <summary>Модель запроса на создание заявки.</summary>
    public record CreateTicketRequest
    {
        /// <summary>Автор заявки (ФИО).</summary>
        [Required] public string AuthorName { get; init; } = default!;

        /// <summary>Категория: IT, электрика, уборка (или прочее).</summary>
        [Required] public string Category { get; init; } = "IT";

        /// <summary>Место: корпус/этаж/аудитория (например, «Ауд. 207»).</summary>
        [Required] public string Place { get; init; } = default!;

        /// <summary>Краткое описание проблемы.</summary>
        [Required] public string Description { get; init; } = default!;

        /// <summary>Приоритет: Low / Medium / High.</summary>
        [Required] public string Priority { get; init; } = "Medium";
    }

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class TicketsController : ControllerBase
    {
        private readonly ITicketRepository _repo;
        public TicketsController(ITicketRepository repo) => _repo = repo;

        /// <summary>Получить список всех заявок.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Ticket>> Get()
            => Ok(_repo.GetAll());

        /// <summary>Получить заявку по идентификатору.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Ticket> GetById(Guid id)
        {
            var found = _repo.GetById(id);
            return found is null ? NotFound() : Ok(found);
        }

        /// <summary>Создать новую заявку (с расчётом SLA).</summary>
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

            // Парсим приоритет в enum (по умолчанию Medium)
            var priorityStr = (req.Priority ?? "Medium").Trim();
            var priority = Enum.TryParse<TicketPriority>(priorityStr, true, out var p)
                ? p : TicketPriority.Medium;

            // Корректировка по приоритету
            var adjust = priority switch
            {
                TicketPriority.Low => +12,
                TicketPriority.High => -12,
                _ => 0
            };

            var hours = Math.Max(6, baseHours + adjust);
            var due = now.AddHours(hours);

            // Создаём заявку ПО ТВОЕЙ МОДЕЛИ (с enum’ами и UpdatedAt)
            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                AuthorName = req.AuthorName,
                Category = category,
                Place = req.Place,
                Description = req.Description,
                Priority = priority,                // enum
                Status = TicketStatus.New,        // enum
                CreatedAt = now,
                UpdatedAt = now,
                SlaDueAt = due
            };

            _repo.Add(ticket);

            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }
    }
}
