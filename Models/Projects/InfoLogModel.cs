using CodeBuggy.Data;
using Microsoft.CodeAnalysis;

namespace CodeBuggy.Models.Projects;

public class InfoLogModel
{
    private static readonly ILogger<InfoLogModel> _logger = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    }).CreateLogger<InfoLogModel>();

    public void StoreInfoLog(AppDbContext context, int projectId, string ticketId, string username, string logMessage, TicketStatus status, LogType logType)
    {

        var info = new InfoLog
        {
            ProjectId = projectId,
            LogMessage = FormatInfoLog(ticketId, username, logMessage, status, logType),
            Timestamp = DateTime.UtcNow,
        };

        _logger.LogInformation("StoreInfoLoginfo: " + projectId.ToString() + "\n" + info.LogMessage + "\n" + info.Timestamp);

        context.InfoLog.Add(info);
        context.SaveChanges();

    }

    private string FormatInfoLog(string ticketId, string username, string logMessage, TicketStatus status, LogType logType)
    {

        string formatted;

        switch (logType)
        {
            case LogType.AddTicket:
                formatted = $"{username} added ticket '{ticketId}'";
                break;

            case LogType.DeleteTicket:
                formatted = $"{username} deleted ticket '{ticketId}'";
                break;

            case LogType.EditTicket:
                formatted = $"{username} edited ticket '{ticketId}'";
                break;

            case LogType.MoveTicket:
                formatted = $"{username} changed the status of ticket '{ticketId}' to '{status}'";
                break;

            case LogType.CommentTicket:
                formatted = $"{username} added a comment on ticket {ticketId}";
                break;

            case LogType.Message:
                formatted = $"{username} : {logMessage}";
                break;

            default:
                formatted = $"Unknown log type: {logType}";
                break;
        }

        return formatted;
    }

    public List<InfoLog> GetInfoLogs(AppDbContext context, int projectId)
    {
        return context.InfoLog.Where(log => log.ProjectId == projectId).ToList();
    }
}