using Microsoft.AspNetCore.Mvc;
using FixItNR.Api.Models;
using FixItNR.Api.Services;
using FixItNR.Api.Data;

namespace FixItNR.Api.Controllers
{
    /// <summary>Запрос: создать заявку и отправить уведомление.</summary>
    public sealed record CreateAndNotifyRequest(
        string AuthorName,
        string Category,
        string Place,
        string Description,
        string Priority,   // строкой: Low/Medium/High
        string EmailTo,
        string? Subject
    );

    /// <summary>Ответ: созданная заявка + результат отправки письма.</summary>
    public sealed record CreateAndNotifyResponse(
        Ticket Ticket,
        EmailSendResult Email
    );

    /// <summary>Единая точка входа платформы: создать заявку и отправить e-mail.</summary>
    [ApiController]
    [Route("api/platform")]
    public sealed class PlatformController : ControllerBase
    {
        private readonly ITicketRepository _repo;
        private readonly IEmailGateway _email;

        public PlatformController(ITicketRepository repo, IEmailGateway email)
        {
            _repo = repo;
            _email = email;
        }

        /// <summary>Создаёт заявку и отправляет уведомление на указанную почту.</summary>
        [HttpPost("tickets")]
        [ProducesResponseType(typeof(CreateAndNotifyResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateAndNotify([FromBody] CreateAndNotifyRequest req, CancellationToken ct)
        {
            // --- 1) Создаём заявку по твоей модели ---
            var now = DateTime.UtcNow;

            var priority = Enum.TryParse<TicketPriority>(req.Priority, true, out var p)
                ? p : TicketPriority.Medium;

            int baseHours = (req.Category ?? "IT").ToLower() switch
            { "it" => 24, "электрика" => 48, "уборка" => 36, _ => 36 };

            int adjust = priority switch
            { TicketPriority.Low => +12, TicketPriority.High => -12, _ => 0 };

            var slaDue = now.AddHours(Math.Max(6, baseHours + adjust));

            var ticket = new Ticket
            {
                AuthorName = req.AuthorName ?? string.Empty,
                Category = req.Category ?? "IT",
                Place = req.Place ?? string.Empty,
                Description = req.Description ?? string.Empty,
                Priority = priority,
                Status = TicketStatus.New,
                CreatedAt = now,
                UpdatedAt = now,
                SlaDueAt = slaDue
            };

            _repo.Add(ticket); // ВАЖНО: тип из FixItNR.Api.Models

            // --- 2) Уведомляем через email-backend ---
            EmailSendResult emailResult;
            try
            {
                emailResult = await _email.SendAsync(
                    new EmailNotifyRequest(
                        EmailTo: req.EmailTo,
                        AuthorName: ticket.AuthorName,
                        Category: ticket.Category,
                        Place: ticket.Place,
                        Description: ticket.Description,
                        Priority: ticket.Priority.ToString(), // enum -> string
                        Subject: string.IsNullOrWhiteSpace(req.Subject)
                            ? $"Новая заявка #{ticket.Id.ToString()[..8]} ({ticket.Category}/{ticket.Priority})"
                            : req.Subject!
                    ),
                    ct);
            }
            catch (Exception ex)
            {
                emailResult = new EmailSendResult(false, $"email error: {ex.Message}");
            }

            var response = new CreateAndNotifyResponse(ticket, emailResult);
            return Created($"/api/tickets/{ticket.Id}", response);
        }
    }
}
