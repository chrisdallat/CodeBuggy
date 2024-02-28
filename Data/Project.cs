namespace CodeBuggy.Data
{
    public class Project
    {
        public Project()
        {
            TicketsCount = 0;
            NotificationCount = 0;
        }

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string AccessCode { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public List<int> TicketsId { get; set; } = new List<int>();
        public int TicketsCount { get; set; }
        public List<int> NotificationIds { get; set; } = new List<int>();
        public int NotificationCount { get; set; }
    }
}
