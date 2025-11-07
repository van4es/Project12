namespace FixItNR.Api.Contracts
{
    public class TicketResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = "New";
        public DateTime CreatedAt { get; set; }
        public DateTime SlaDueAt { get; set; }
        public string Message { get; set; } = string.Empty; // пояснение расчёта SLA
    }
}