namespace CodeBuggy.Data
{
    public class InfoLog
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string? Username { get; set; }
        public string? LogMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum LogType
    {
        AddTicket,
        DeleteTicket,
        EditTicket,
        MoveTicket,
        CommentTicket, //TODO: add when commenting functionality implemented
        Message,
    }
}