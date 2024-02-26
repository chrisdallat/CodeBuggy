using CodeBuggy.Controllers;
using CodeBuggy.Data;
using CodeBuggy.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
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
    private readonly InfoLogModel _infoLogModel;

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
        _infoLogModel = new InfoLogModel();
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

        var projectDetails = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (projectDetails == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        var owner = _userManager?.GetUserAsync(user)?.Result?.FirstName + " " + _userManager?.GetUserAsync(user)?.Result?.LastName;
        if (owner == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        projectDetails.TicketsCount += 1;
        var ticketDetails = new Ticket
        {
            StringId = $"{projectDetails.Name.ToUpper()}-{projectDetails.TicketsCount}",
            Title = input.TicketTitle,
            Priority = input.TicketPriorityValue >= TicketPriority.None ? input.TicketPriorityValue : TicketPriority.None,
            Status = input.TicketStatusValue >= TicketStatus.ToDo ? input.TicketStatusValue : TicketStatus.ToDo,
            CreatedBy = owner,
            CreationDate = DateTime.UtcNow,
            ResolvedBy = "",
            Description = input.TicketDescription,
            Comments = "",
        };

        _context.Tickets.Add(ticketDetails);
        await _context.SaveChangesAsync();

        if (ticketDetails?.Id == null || ticketDetails.Id == 0)
        {
            return new OperationResult { Success = false, Message = "Unable to create new ticket" };
        }

        projectDetails.TicketsId.Add(ticketDetails.Id);
        await _context.SaveChangesAsync();
        return new OperationResult { Success = true, Message = "Ticket created successfully!" };
    }

    public bool PopulateTickets(int projectId)
    {
        try
        {
            var project = _context.Projects.FirstOrDefault(p => p.Id == projectId);

            if (project == null)
            {
                return false;
            }

            var tickets = _context.Tickets
                .Where(t => project.TicketsId.Contains(t.Id))
                .ToList();

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

        try
        {
            await _context.SaveChangesAsync();

            _burndownModel.StoreBurndownData(_context, projectId);

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

        ticketDetails.Priority = input.TicketPriorityValue;
        ticketDetails.Status = input.TicketStatusValue;
        ticketDetails.Description = input.TicketDescription;
        ticketDetails.Title = input.TicketTitle;

        try
        {
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

        try
        {
            _context.Tickets.Remove(ticket);
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
}
