namespace CodeBuggy.Data
{
    public class Notification
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum NotificationType
    {
        AddTicket,
        DeleteTicket,
        EditTicket,
        MoveTicket,
        CommentTicket, //TODO: add when commenting functionality implemented
        Message,
        ChangeAssignee,
    }
}