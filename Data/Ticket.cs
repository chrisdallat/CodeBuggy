namespace CodeBuggy.Data
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Username { get; set; }
        public DateTime Timestamp { get; set; }
        public int TicketId { get; set; }
    }

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
        public List<Comment> Comments { get; set; } = new List<Comment>();
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
