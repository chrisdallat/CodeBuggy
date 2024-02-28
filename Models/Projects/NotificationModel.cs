using CodeBuggy.Data;
using Microsoft.CodeAnalysis;

namespace CodeBuggy.Models.Projects;

public class NotificationModel
{
    private static readonly ILogger<NotificationModel> _logger = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    }).CreateLogger<NotificationModel>();

    public int StoreNotification(AppDbContext context, int projectId, string ticketId, string username, string message, TicketStatus ticketStatus, NotificationType notificationType)
    {
        var status = FormatStatus(ticketStatus);
        var info = new Notification
        {
            Message = FormatNotification(ticketId, username, message, status, notificationType),
            Timestamp = DateTime.UtcNow,
        };
        
        context.Notifications.Add(info);
        context.SaveChanges();

        return info.Id;
    }

    private string FormatNotification(string ticketId, string username, string message, string status, NotificationType notificationType)
    {

        string formatted;

        switch (notificationType)
        {
            case NotificationType.AddTicket:
                formatted = $"{username} added ticket {ticketId}";
                break;

            case NotificationType.DeleteTicket:
                formatted = $"{username} deleted ticket {ticketId}";
                break;

            case NotificationType.EditTicket:
                formatted = $"{username} edited ticket {ticketId}";
                break;

            case NotificationType.MoveTicket:
                formatted = $"{username} changed the status of ticket {ticketId} to {status}";
                break;

            case NotificationType.CommentTicket:
                formatted = $"{username} added a comment on ticket {ticketId}";
                break;

            case NotificationType.Message:
                formatted = $"{username} : {message}";
                break;

            default:
                formatted = $"Unknown log type: {notificationType}";
                break;
        }

        return formatted;
    }

    public string FormatStatus(TicketStatus status)
    {

        string formattedStatus;

        switch (status)
        {
            case TicketStatus.ToDo:
                formattedStatus = "TO DO";
                break;
            case TicketStatus.InProgress:
                formattedStatus = "IN PROGRESS";
                break;
            case TicketStatus.Review:
                formattedStatus = "REVIEW";
                break;
            case TicketStatus.Done:
                formattedStatus = "DONE";
                break;
            default:
                formattedStatus = "UNKNOWN STATUS";
                break;
        }

        return formattedStatus;
    }

    public List<Notification> GetNotificationData(AppDbContext context, int projectId)
    {
        try
        {
            var project = context.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return new List<Notification>();
            }

            var notifications = context.Notifications
                .Where(t => project.NotificationIds.Contains(t.Id))
                .ToList();

            return notifications;
        }
        catch (Exception ex)
        {
            return new List<Notification>();
        }
    }

}