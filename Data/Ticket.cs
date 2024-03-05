namespace CodeBuggy.Data
{

    public class Ticket
    {
        private static int ticketCounter = 1;

        public int Id { get; set; }
        public string StringId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string ResolvedBy { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<int>? CommentsIds { get; set; }
        public int? CommentsCount { get; set; }
    }
    public enum TicketStatus
    {
        ToDo,
        InProgress,
        Review,
        Done
    }

    public enum TicketPriority
    { 
        None, 
        Low,
        Medium,
        High,
        Urgent
    }
}
