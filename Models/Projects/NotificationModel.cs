using CodeBuggy.Data;
using Microsoft.CodeAnalysis;

namespace CodeBuggy.Models.Projects;

public class NotificationModel
{
    private static readonly ILogger<NotificationModel> _logger = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    }).CreateLogger<NotificationModel>();

    public int StoreNotification(AppDbContext context, int projectId, string ticketStringId, string username, string message, TicketStatus ticketStatus, NotificationType notificationType, int ticketId)
    {
        var status = FormatStatus(ticketStatus);
        var info = new Notification
        {
            Message = FormatNotification(ticketStringId, username, message, status, notificationType, ticketId),
            Timestamp = DateTime.UtcNow,
        };
        
        context.Notifications.Add(info);
        context.SaveChanges();

        return info.Id;
    }

    private string FormatNotification(string ticketStringId, string username, string message, string status, NotificationType notificationType, int ticketId)
    {

        string formatted;

        switch (notificationType)
        {
            case NotificationType.AddTicket:
                formatted = $"{username} added ticket <p id='ticketStringId' onclick=openTicketPopup({ticketId})>{ticketStringId}</p>";
                break;

            case NotificationType.DeleteTicket:
                formatted = $"{username} deleted ticket <p id='ticketStringId' onclick=openTicketPopup({ticketId})>{ticketStringId}</p>";
                break;

            case NotificationType.EditTicket:
                formatted = $"{username} edited ticket <p id='ticketStringId' onclick=openTicketPopup({ticketId})>{ticketStringId}</p>";
                break;

            case NotificationType.MoveTicket:
                formatted = $"{username} changed the status of ticket <p id='ticketStringId' onclick=openTicketPopup({ticketId})>{ticketStringId}</p> to {status}";
                break;

            case NotificationType.CommentTicket:
                formatted = $"{username} added a comment on ticket <p id='ticketStringId' onclick=openTicketPopup({ticketId})>{ticketStringId}</p>";
                break;

            case NotificationType.Message:
                formatted = $"{username} : {message}";
                break;

            case NotificationType.ChangeAssignee:
                formatted = $"<p id='ticketStringId' onclick=openTicketPopup({ticketId})>{ticketStringId}</p> is assigned now to {username}";
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