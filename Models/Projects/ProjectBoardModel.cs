using CodeBuggy.Controllers;
using CodeBuggy.Data;
using CodeBuggy.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
using System.Security.Claims;

namespace CodeBuggy.Models.Projects;

public class ProjectBoardModel
{
    private readonly ILogger<ProjectsController> _logger;
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly BurndownModel _burndownModel;
    private readonly NotificationModel _notificationModel;

    public List<Ticket> Tickets { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(255, ErrorMessage = "The Ticket description must be maximum 255 characters.")]
        [Display(Name = "Ticket title")]
        public string TicketTitle { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Ticket description")]
        public string TicketDescription { get; set; } = string.Empty;

        public TicketPriority TicketPriorityValue { get; set; }

        public TicketStatus TicketStatusValue { get; set; }

        public string TicketComments { get; set; } = string.Empty;

    }

    public ProjectBoardModel(ILogger<ProjectsController> logger, AppDbContext context, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _burndownModel = new BurndownModel();
        _notificationModel = new NotificationModel();
    }

    public async Task<OperationResult> AddTicketToProject(ClaimsPrincipal user, InputModel input, int projectId)
    {
        if (user.Identity == null || user.Identity.IsAuthenticated == false)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        if (ValidUserClaim(user, projectId) == false)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        if (string.IsNullOrWhiteSpace(input.TicketTitle))
        {
            return new OperationResult { Success = false, Message = "Ticket title must be provided" };
        }

        if ((input.TicketPriorityValue >= TicketPriority.None && input.TicketPriorityValue <= TicketPriority.Urgent) == false)
        {
            return new OperationResult { Success = false, Message = "Priority is invalid!" };
        }
        if ((input.TicketStatusValue >= TicketStatus.ToDo && input.TicketStatusValue <= TicketStatus.Backlog) == false)
        {
            return new OperationResult { Success = false, Message = "Status is invalid!" };
        }


        var projectDetails = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (projectDetails == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        var username = GetUsername(user);
        if (username == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        try
        {
            projectDetails.TicketsCount += 1;
            var ticketDetails = new Ticket
            {
                StringId = $"{projectDetails.Name.ToUpper()}-{projectDetails.TicketsCount}",
                Title = input.TicketTitle,
                Priority = input.TicketPriorityValue >= TicketPriority.None ? input.TicketPriorityValue : TicketPriority.None,
                Status = input.TicketStatusValue >= TicketStatus.ToDo ? input.TicketStatusValue : TicketStatus.ToDo,
                Reporter = username,
                CreationDate = DateTime.UtcNow,
                Assignee = username,
                ResolvedBy = "",
                Description = input.TicketDescription,
                CommentsIds = new List<int>(),
                CommentsCount = 0,
            };

            _context.Tickets.Add(ticketDetails);
            await _context.SaveChangesAsync();

            var notificationId = _notificationModel.StoreNotification(_context, projectId, ticketDetails.StringId, username, "", ticketDetails.Status, NotificationType.AddTicket, ticketDetails.Id);

            if (ticketDetails?.Id == null || ticketDetails.Id == 0)
            {
                return new OperationResult { Success = false, Message = "Unable to create new ticket" };
            }


            projectDetails.TicketsId.Add(ticketDetails.Id);
            projectDetails.NotificationIds.Add(notificationId);
            projectDetails.NotificationCount += 1;
            await _context.SaveChangesAsync();

            return new OperationResult { Success = true, Message = "Ticket created successfully!" };
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving tickets: " + ex);
            return new OperationResult { Success = false, Message = "Ticket was not created!" };
        }

    }

    public bool PopulateTickets(int projectId, ClaimsPrincipal user, bool userFilter)
    {
        try
        {
            var project = _context.Projects.FirstOrDefault(p => p.Id == projectId);

            if (project == null)
            {
                return false;
            }

            var username = GetUsername(user);
            if (username == null)
            {
                return false;
            }

            List<Ticket> tickets;

            if (userFilter == true)
            {
                tickets = _context.Tickets
                .Where(t => project.TicketsId.Contains(t.Id) && t.Assignee == username)
                .ToList();
            }
            else 
            {
                tickets = _context.Tickets
                .Where(t => project.TicketsId.Contains(t.Id))
                .ToList();
            }

            _burndownModel.StoreBurndownData(_context, projectId);

            Tickets = tickets;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving tickets: " + ex);
            return false;
        }

        return true;
    }

    public bool ValidUserClaim(ClaimsPrincipal user, int projectId)
    {
        var userId = _userManager?.GetUserId(user);
        if (userId == null)
        {
            return false;
        }

        var foundUserClaim = _context.UserClaims.FirstOrDefault(p => p.ClaimValue == projectId.ToString() && p.ClaimType == "ProjectAccess" && p.UserId == userId);
        if (foundUserClaim == null)
        {
            return false;
        }

        return true;
    }

    public string? GetProjectName(int projectId)
    {
        var project = _context.Projects.FirstOrDefault(p => p.Id == projectId);

        if (project == null)
        {
            return null;
        }

        return project.Name;
    }

    public async Task<OperationResult> ChangeTicketStatus(ClaimsPrincipal user, int projectId, int ticketId, string status)
    {
        if (ValidUserClaim(user, projectId) == false)
        {
            return new OperationResult { Success = false, Message = "User Doesn't have access" };
        }

        var ticketDetails = await _context.Tickets.FirstOrDefaultAsync(p => p.Id == ticketId);
        if (ticketDetails == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        ticketDetails.Status = Enum.Parse<TicketStatus>(status);
        var username = GetUsername(user);
        if (username == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        var projectDetails = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (projectDetails == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        try
        {
            await _context.SaveChangesAsync();

            var notificationId = _notificationModel.StoreNotification(_context, projectId, ticketDetails.StringId, username, "", ticketDetails.Status, NotificationType.MoveTicket, ticketDetails.Id);
            projectDetails.NotificationIds.Add(notificationId);
            projectDetails.NotificationCount += 1;
            _burndownModel.StoreBurndownData(_context, projectId);
            await _context.SaveChangesAsync();

            return new OperationResult { Success = true, Message = $"Status of ticket changed successfully to {status}." };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error changing status of ticket with ID {ticketId}: {ex.Message}");

            return new OperationResult { Success = false, Message = $"Error changing status of ticket : {ex.Message}" };
        }
    }

    public async Task<OperationResult> SaveTicketChanges(ClaimsPrincipal user, int projectId, int ticketId, InputModel input)
    {
        if (ValidUserClaim(user, projectId) == false)
        {
            return new OperationResult { Success = false, Message = "User Doesn't have access" };
        }

        var ticketDetails = await _context.Tickets.FirstOrDefaultAsync(p => p.Id == ticketId);
        if (ticketDetails == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        if (string.IsNullOrWhiteSpace(input.TicketTitle))
        {
            return new OperationResult { Success = false, Message = "Title must be provided" };
        }

        bool ticketChange = TicketChanges(ticketDetails, input);
        bool statusChange = false;

        if (ticketDetails.Status != input.TicketStatusValue)
        {
            ticketDetails.Status = input.TicketStatusValue;
            statusChange = true;
        }

        if (ticketChange == false && statusChange == false)
        {
            return new OperationResult { Success = false, Message = "No changes made to ticket" };
        }

        var username = GetUsername(user);
        if (username == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        var projectDetails = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (projectDetails == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }
        try
        {
            await _context.SaveChangesAsync();

            var notificationId = 0;

            if (ticketChange == true)
            {
                notificationId = _notificationModel.StoreNotification(_context, projectId, ticketDetails.StringId, username, "", ticketDetails.Status, NotificationType.EditTicket, ticketDetails.Id);
                projectDetails.NotificationIds.Add(notificationId);
                projectDetails.NotificationCount += 1;
            }

            if (statusChange == true)
            {
                notificationId = _notificationModel.StoreNotification(_context, projectId, ticketDetails.StringId, username, "", ticketDetails.Status, NotificationType.MoveTicket, ticketDetails.Id);
                projectDetails.NotificationIds.Add(notificationId);
                projectDetails.NotificationCount += 1;
            }

            await _context.SaveChangesAsync();

            _burndownModel.StoreBurndownData(_context, projectId);

            return new OperationResult { Success = true, Message = $"Ticket details changed successfully." };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error changing status of ticket with ID {ticketId}: {ex.Message}");

            return new OperationResult { Success = false, Message = $"Error changing ticket details : {ex.Message}" };
        }
    }

    public bool TicketChanges(Ticket ticket, InputModel input)
    {
        bool changed = false;

        if (ticket.Priority != input.TicketPriorityValue)
        {
            ticket.Priority = input.TicketPriorityValue;
            changed = true;
        }

        if (ticket.Description != input.TicketDescription)
        {
            ticket.Description = input.TicketDescription;
            changed = true;
        }

        if (ticket.Title != input.TicketTitle)
        {
            ticket.Title = input.TicketTitle;
            changed = true;
        }

        return changed;
    }


    public async Task<OperationResult> DeleteTicket(ClaimsPrincipal user, int projectId, int ticketId)
    {
        if (ValidUserClaim(user, projectId) == false)
        {
            return new OperationResult { Success = false, Message = "User Doesn't have access" };
        }

        var ticket = await _context.Tickets.FirstOrDefaultAsync(p => p.Id == ticketId);
        if (ticket == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        var username = GetUsername(user);
        if (username == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        var projectDetails = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (projectDetails == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        try
        {
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            var notificationId = _notificationModel.StoreNotification(_context, projectId, ticket.StringId, username, "", ticket.Status, NotificationType.DeleteTicket, ticket.Id);
            projectDetails.NotificationIds.Add(notificationId);
            projectDetails.NotificationCount += 1;
            await _context.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            _logger.LogError("Error: " + ex);
            return new OperationResult { Success = false, Message = "Error: " + ex.Message };
        }

        return new OperationResult { Success = true, Message = "Ticket deleted" };

    }

    public async Task<int> GetTicketStatus(ClaimsPrincipal user, int projectId, int ticketId)
    {
        if (ValidUserClaim(user, projectId) == false)
        {
            return -1;
        }

        var ticketDetails = await _context.Tickets.FirstOrDefaultAsync(p => p.Id == ticketId);
        if (ticketDetails == null)
        {
            return -1;
        }

        return (int)ticketDetails.Status;
    }

    public async Task<Ticket>? GetTicketInfo(ClaimsPrincipal user, int projectId, int ticketId)
    {
        if (ValidUserClaim(user, projectId) == false)
        {
            return null;
        }

        var ticketDetails = await _context.Tickets.FirstOrDefaultAsync(p => p.Id == ticketId);
        
        return ticketDetails;
    }

    public async Task<OperationResult> AddCommentToTicket(ClaimsPrincipal user, int projectId, int ticketId, string comment)
    {
        if (ValidUserClaim(user, projectId) == false)
        {
            return new OperationResult { Success = false, Message = "User Doesn't have access" };
        }

        var ticket = await _context.Tickets.FirstOrDefaultAsync(p => p.Id == ticketId);
        if (ticket == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        var projectDetails = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (projectDetails == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        var username = GetUsername(user);
        if (username == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        var commentDetails = new Comment
        {
            Text = comment,
            Username = username,
            Timestamp = DateTime.UtcNow,
        };

        try
        {
            _context.Comments.Add(commentDetails);
            await _context.SaveChangesAsync();

            ticket.CommentsCount += 1;
            ticket.CommentsIds.Add(commentDetails.Id);

            var notificationId = _notificationModel.StoreNotification(_context, projectId, ticket.StringId, username, "", ticket.Status, NotificationType.CommentTicket, ticket.Id);

            projectDetails.NotificationIds.Add(notificationId);
            projectDetails.NotificationCount += 1;
            await _context.SaveChangesAsync();


        }
        catch (Exception ex)
        {
            _logger.LogError("Error: " + ex);
            return new OperationResult { Success = false, Message = "Error: " + ex.Message };
        }

        return new OperationResult { Success = true, Message = "Comment added successfully!" };
    }

    public OperationResult GetComments(ClaimsPrincipal user, int projectId, int ticketId, ref List<Comment> comments)
    {
        if (ValidUserClaim(user, projectId) == false)
        {
            return new OperationResult { Success = false, Message = "User Doesn't have access" };
        }

        var ticket = _context.Tickets.FirstOrDefault(p => p.Id == ticketId);
        if (ticket == null || ticket.CommentsIds == null)
        {
            return new OperationResult { Success = false, Message = "Ticket not found" };
        }

        comments = _context.Comments
                .Where(t => ticket.CommentsIds.Contains(t.Id))
                .OrderByDescending(c => c.Timestamp)
                .ToList();

        return new OperationResult { Success = true, Message = "Found comments" };
    }

    public string? GetUsername(ClaimsPrincipal user)
    {
        var username = _userManager?.GetUserAsync(user)?.Result?.FirstName + " " + _userManager?.GetUserAsync(user)?.Result?.LastName;
        if (username == null)
        {
            return null;
        }

        return username;
    }

    // TODO: Implement the return properly
    public List<Ticket> GetSearchResults(ClaimsPrincipal user, int projectId, string query)
    {
        if (!ValidUserClaim(user, projectId))
        {
            return new List<Ticket>();
        }

        var project = _context.Projects.FirstOrDefault(p => p.Id == projectId);
        if (project == null)
        {
            return new List<Ticket>(); // or return null/exception maybe??
        }

        var searchResults = _context.Tickets
        .Where(t => project.TicketsId.Contains(t.Id) &&
                    (t.StringId.ToLower().Contains(query.ToLower()) ||
                    t.Title.ToLower().Contains(query.ToLower()) ||
                    (t.Description != null && t.Description.ToLower().Contains(query.ToLower()))))
        .ToList();

        return searchResults;
    }

    public async Task<OperationResult> AssignTicketToUser(ClaimsPrincipal user, int projectId, int ticketId)
    {
        if (!ValidUserClaim(user, projectId))
        {
            return new OperationResult { Success = false, Message = "User Doesn't have access" };
        }

        var username = GetUsername(user);
        if (username == null)
        {
            return new OperationResult { Success = false, Message = "Encountered an error!" }; 
        }

        var ticket = await _context.Tickets.FirstOrDefaultAsync(p => p.Id == ticketId);
        if (ticket == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        var projectDetails = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (projectDetails == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        ticket.Assignee = username;

        try
        {
            await _context.SaveChangesAsync();
            var notificationId = _notificationModel.StoreNotification(_context, projectId, ticket.StringId, username, "", ticket.Status, NotificationType.ChangeAssignee, ticket.Id);

            projectDetails.NotificationIds.Add(notificationId);
            projectDetails.NotificationCount += 1;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: " + ex);
            return new OperationResult { Success = false, Message = "Error: " + ex.Message };
        }

        return new OperationResult { Success = true, Message = username };
    }

}
