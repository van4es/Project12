namespace FixItNR.Api.Models
{
    public enum TicketStatus { New, InProgress, Resolved, Closed }
    public enum TicketPriority { Low, Medium, High }


    public class Ticket
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string AuthorName { get; set; } = string.Empty;
        public string Category { get; set; } = "IT"; // IT, Электрика, Уборка, Прочее
        public string Place { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        public TicketStatus Status { get; set; } = TicketStatus.New;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime SlaDueAt { get; set; } // вычисляется при создании
    }
}