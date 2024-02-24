using CodeBuggy.Controllers;
using CodeBuggy.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace CodeBuggy.Models.Projects;

public class BurndownModel
{
    private static readonly ILogger<BurndownModel> _logger = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    }).CreateLogger<BurndownModel>();

    public void StoreBurndownData(AppDbContext context, int projectId)
    {
        try
        {
            var project = context.Projects.FirstOrDefault(p => p.Id == projectId);

            if (project == null)
            {
                _logger.LogError("Project not found.");
                return;
            }

            var tickets = context.Tickets
                .Where(t => project.TicketsId.Contains(t.Id))
                .ToList();

            var currentDate = DateTime.UtcNow.Date;

            var ticketGroups = tickets
                .GroupBy(t => new { t.Status, t.Priority })
                .Select(group => new
                {
                    Status = group.Key.Status,
                    Priority = group.Key.Priority,
                    Count = group.Count()
                });

            // Check if theres already data today
            var existingData = context.BurndownData
                .Where(d => d.ProjectId == projectId && d.DailyCounts.Any(dc => dc.Date == currentDate))
                .FirstOrDefault();

            if (existingData != null)
            {
                existingData.DailyCounts.First().ToDoCount = ticketGroups.Where(g => g.Status == TicketStatus.ToDo).Sum(g => g.Count);
                existingData.DailyCounts.First().InProgressCount = ticketGroups.Where(g => g.Status == TicketStatus.InProgress).Sum(g => g.Count);
                existingData.DailyCounts.First().ReviewCount = ticketGroups.Where(g => g.Status == TicketStatus.Review).Sum(g => g.Count);
                existingData.DailyCounts.First().DoneCount = ticketGroups.Where(g => g.Status == TicketStatus.Done).Sum(g => g.Count);
                existingData.DailyCounts.First().NonePriorityCount = ticketGroups.Where(g => g.Priority == TicketPriority.None).Sum(g => g.Count);
                existingData.DailyCounts.First().LowPriorityCount = ticketGroups.Where(g => g.Priority == TicketPriority.Low).Sum(g => g.Count);
                existingData.DailyCounts.First().MediumPriorityCount = ticketGroups.Where(g => g.Priority == TicketPriority.Medium).Sum(g => g.Count);
                existingData.DailyCounts.First().HighPriorityCount = ticketGroups.Where(g => g.Priority == TicketPriority.High).Sum(g => g.Count);
                existingData.DailyCounts.First().UrgentPriorityCount = ticketGroups.Where(g => g.Priority == TicketPriority.Urgent).Sum(g => g.Count);

                context.BurndownData.Update(existingData);
            }
            else
            {
                var newData = new BurndownData
                {
                    ProjectId = projectId,
                    DailyCounts = new List<DailyTicketCounts>
                    {
                        new DailyTicketCounts
                        {
                            Date = currentDate,
                            ToDoCount = ticketGroups.Where(g => g.Status == TicketStatus.ToDo).Sum(g => g.Count),
                            InProgressCount = ticketGroups.Where(g => g.Status == TicketStatus.InProgress).Sum(g => g.Count),
                            ReviewCount = ticketGroups.Where(g => g.Status == TicketStatus.Review).Sum(g => g.Count),
                            DoneCount = ticketGroups.Where(g => g.Status == TicketStatus.Done).Sum(g => g.Count),
                            NonePriorityCount = ticketGroups.Where(g => g.Priority == TicketPriority.None).Sum(g => g.Count),
                            LowPriorityCount = ticketGroups.Where(g => g.Priority == TicketPriority.Low).Sum(g => g.Count),
                            MediumPriorityCount = ticketGroups.Where(g => g.Priority == TicketPriority.Medium).Sum(g => g.Count),
                            HighPriorityCount = ticketGroups.Where(g => g.Priority == TicketPriority.High).Sum(g => g.Count),
                            UrgentPriorityCount = ticketGroups.Where(g => g.Priority == TicketPriority.Urgent).Sum(g => g.Count),
                        }
                    }
                };

                context.BurndownData.Add(newData);
            }

            context.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error storing Burndown data: " + ex);
        }
    }

    public List<DailyTicketCounts> GetBurndownData(AppDbContext context, int projectId)
    {
        try
        {
            var burndownData = context.BurndownData
                .Where(d => d.ProjectId == projectId)
                .Include(d => d.DailyCounts)
                .ToList();

            if (burndownData.Any())
            {
                // Concatenate all DailyCounts from different BurndownData entities
                var allDailyCounts = burndownData.SelectMany(d => d.DailyCounts).ToList();
                _logger.LogInformation("Burndown Data: " + JsonConvert.SerializeObject(allDailyCounts));

                return allDailyCounts;
            }

            return new List<DailyTicketCounts>();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving Burndown data: " + ex);
            return null;
        }
    }

    public bool ValidUserClaim(AppDbContext context, UserManager<AppUser> userManager, ClaimsPrincipal user, int projectId)
    {
        var userId = userManager?.GetUserId(user);
        if (userId == null)
        {
            return false;
        }

        var foundUserClaim = context.UserClaims.FirstOrDefault(p => p.ClaimValue == projectId.ToString() && p.ClaimType == "ProjectAccess" && p.UserId == userId);
        if (foundUserClaim == null)
        {
            return false;
        }

        return true;
    }
}