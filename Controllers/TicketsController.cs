using Microsoft.AspNetCore.Mvc;

namespace FixItNR.Api.Controllers
{
    public record CreateTicketRequest(string AuthorName, string Category, string Place, string Description, string Priority);
    public record Ticket(
        Guid Id, string AuthorName, string Category, string Place, string Description, string Priority,
        DateTime CreatedAt, DateTime SlaDueAt, string Status
    );

    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private static readonly List<Ticket> _tickets = new();

        [HttpGet]
        public ActionResult<IEnumerable<Ticket>> Get() => Ok(_tickets);

        [HttpPost]
        public ActionResult<Ticket> Post([FromBody] CreateTicketRequest req)
        {
            var now = DateTime.UtcNow;
            int baseHours = (req.Category ?? "IT").ToLower() switch
            { "it" => 24, "электрика" => 48, "уборка" => 36, _ => 36 };
            int adjust = (req.Priority ?? "Medium").ToLower() switch
            { "low" => +12, "high" => -12, _ => 0 };
            var due = now.AddHours(Math.Max(6, baseHours + adjust));

            var t = new Ticket(Guid.NewGuid(), req.AuthorName ?? "", req.Category ?? "IT", req.Place ?? "",
                req.Description ?? "", req.Priority ?? "Medium", now, due, "New");
            _tickets.Add(t);
            return CreatedAtAction(nameof(Get), new { id = t.Id }, t);
        }
    }
}
