namespace CodeBuggy.Data
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; }
    }

    public enum TicketStatus
    {
        ToDo,
        InProgress,
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
