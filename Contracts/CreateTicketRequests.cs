namespace FixItNR.Api.Contracts
{
    public class CreateTicketRequest
    {
        public string AuthorName { get; set; } = string.Empty;
        public string Category { get; set; } = "IT";
        public string Place { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium"; // Low | Medium | High
    }
}