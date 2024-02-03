using System;
using System.Collections.Generic;

namespace CodeBuggy.Data
{
    public class DailyTicketCounts
    {
        public DateTime Date { get; set; }
        public int ToDoCount { get; set; }
        public int InProgressCount { get; set; }
        public int ReviewCount { get; set; }
        public int DoneCount { get; set; }
        public int NonePriorityCount { get; set; }
        public int LowPriorityCount { get; set; }
        public int MediumPriorityCount { get; set; }
        public int HighPriorityCount { get; set; }
        public int UrgentPriorityCount { get; set; }
    }

    public class BurndownData
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public List<DailyTicketCounts>? DailyCounts { get; set; }
    }
}
