using CodeBuggy.Controllers;
using CodeBuggy.Data;
using CodeBuggy.Helpers;
using Microsoft.AspNetCore.Mvc;
using PagedList;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Security;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace CodeBuggy.Models.Projects;

public class ProjectsModel
{
    private readonly ILogger<ProjectsController> _logger;
    private readonly AppDbContext _context;
    private static readonly Random Random = new Random();
    private readonly UserManager<AppUser> _userManager;

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(255, ErrorMessage = "The Project name must have max 255 characters.")]
        [Display(Name = "Project name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(255, ErrorMessage = "The access code must have max 255 characters.")]
        [Display(Name = "Access code")]
        public string AccessCode { get; set; } = string.Empty;
    }

    public string GenerateAccessCode()
    {
        char[] accessCodeGenerated = new char[32];
        string accessCode;
        const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        do
        {
            for (int i = 0; i < 32; i++)
            {
                accessCodeGenerated[i] = Characters[Random.Next(Characters.Length)];
            }

            accessCode = new string(accessCodeGenerated);

        } while (_context.Projects.Any(p => p.AccessCode == accessCode));

        return accessCode;
    }


    public ProjectsModel(ILogger<ProjectsController> logger, AppDbContext context, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    private List<Project>? GetProjects(ClaimsPrincipal user)
    {
        string? userId = _userManager.GetUserId(user);
        if (userId == null)
        {
            return null;
        }

        var projectIds = _context.UserClaims 
        .Where(uc => uc.UserId == userId && uc.ClaimType == "ProjectAccess") 
        .Select(uc => uc.ClaimValue) 
        .ToList();

        
        // Filter projects based on the user's claims
        var projectList = _context.Projects
            .Where(p => projectIds.Contains(p.Id.ToString()))
            .Select(e => new Project
            {
                Id = e.Id,
                Name = e.Name,
                AccessCode = e.AccessCode,
                Owner = e.Owner,
                OwnerId = e.OwnerId,
            })
            .ToList();

        return projectList;
    }

    public IPagedList<Project>? GetProjectsList(int page, ClaimsPrincipal user)
    {
        int pageSize = 6;

        if (user.Identity == null || user.Identity.IsAuthenticated == false)
        {
            return null;
        }

        // Get the user's Id from the ClaimsPrincipal
        string? userId = _userManager.GetUserId(user);
        if (userId == null)
        {
            return null;
        }

        try
        {

            var projectList = GetProjects(user).ToPagedList(page, pageSize);

            if (projectList != null && projectList.Any())
            {
                return projectList;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError("Couldn't load projects: " + ex);
        }

        return null;
    }


    public List<Ticket>? GetTickets(ClaimsPrincipal user, int projectId)
    {
        if (user.Identity != null && user.Identity.IsAuthenticated)
        {
            List<Ticket> tickets = new List<Ticket>
            {
                new Ticket { Id = 1, Title = "Task 1", Description = "Description 1", Priority = TicketPriority.None, Status = TicketStatus.ToDo },
                new Ticket { Id = 2, Title = "Task 2", Description = "Description 2", Priority = TicketPriority.Low, Status = TicketStatus.InProgress },
                new Ticket { Id = 3, Title = "Task 3", Description = "Description 3", Priority = TicketPriority.Medium, Status = TicketStatus.Done },
                new Ticket { Id = 4, Title = "Task 4", Description = "Description 4", Priority = TicketPriority.High, Status = TicketStatus.Done },
                new Ticket { Id = 5, Title = "Task 5", Description = "Description 5", Priority = TicketPriority.Urgent, Status = TicketStatus.ToDo },
                new Ticket { Id = 6, Title = "Task 6", Description = "Description 6", Priority = TicketPriority.None, Status = TicketStatus.InProgress },
                new Ticket { Id = 7, Title = "Task 7", Description = "Description 7", Priority = TicketPriority.Low, Status = TicketStatus.ToDo },
                new Ticket { Id = 8, Title = "Task 8", Description = "Description 8", Priority = TicketPriority.Medium, Status = TicketStatus.Done },
                new Ticket { Id = 9, Title = "Task 9", Description = "Description 9", Priority = TicketPriority.High, Status = TicketStatus.InProgress },
                new Ticket { Id = 10, Title = "Task 10", Description = "Description 10", Priority = TicketPriority.Urgent, Status = TicketStatus.InProgress },
                new Ticket { Id = 11, Title = "Task 11", Description = "Description 11", Priority = TicketPriority.None, Status = TicketStatus.ToDo },
                new Ticket { Id = 12, Title = "Task 12", Description = "Description 12", Priority = TicketPriority.Low, Status = TicketStatus.Done },
                new Ticket { Id = 13, Title = "Task 13", Description = "Description 13", Priority = TicketPriority.Medium, Status = TicketStatus.Done },
                new Ticket { Id = 14, Title = "Task 14", Description = "Description 14", Priority = TicketPriority.High, Status = TicketStatus.ToDo },
                new Ticket { Id = 15, Title = "Task 15", Description = "Description 15", Priority = TicketPriority.Urgent, Status = TicketStatus.InProgress },
            };

            return tickets;
        }

        return null;
    }

    public async Task<OperationResult> AddExistingProject(InputModel input, ClaimsPrincipal user)
    {
        if (user.Identity == null || user.Identity.IsAuthenticated == false)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }
        
        var existingProject = _context.Projects.FirstOrDefault(p => p.Name == input.Name && p.AccessCode == input.AccessCode);

        if (existingProject == null)
        {
            return new OperationResult { Success = false, Message = "Project does not exist or not found!" };
        }

        var userId = _userManager?.GetUserId(user);
        if (userId == null || existingProject?.Id == null || existingProject.Id == 0)
        {
            return new OperationResult { Success = false, Message = "Could not join existing project" };
        }

        var projectClaimed = _context.UserClaims.FirstOrDefault(p => p.ClaimValue == existingProject.Id.ToString() && p.ClaimType == "ProjectAccess");
        if (projectClaimed != null)
        {
            return new OperationResult { Success = false, Message = "You already have access to this project" };
        }

        bool result = AddProjectClaim(existingProject.Id, userId).Success;
        return new OperationResult { Success = result, Message = "Project was found and added! "};
    }

    public async Task<OperationResult> AddNewProjectAsync(InputModel input, ClaimsPrincipal user)
    {
        if (user.Identity == null || user.Identity.IsAuthenticated == false)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        var owner = _userManager?.GetUserAsync(user)?.Result?.FirstName + " " + _userManager?.GetUserAsync(user)?.Result?.LastName;
        var userId = _userManager?.GetUserId(user);

        if (owner == null || userId == null)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        try
        {
            var userProjects = GetProjects(user);
            if (userProjects != null)
            {
                var existingProjectWithName = userProjects.FirstOrDefault(p => p.Name == input.Name && p.OwnerId == userId);

                if (existingProjectWithName != null)
                {
                    return new OperationResult { Success = false, Message = "Project with the same name already exists" };
                }
            }

            string accessCode = GenerateAccessCode();

            var newProject = new Project
            {
                Name = input.Name,
                Owner = owner,
                AccessCode = accessCode,
                OwnerId = userId,
            };

            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();

            if (userId == null || newProject?.Id == null || newProject.Id == 0)
            {
                return new OperationResult { Success = false, Message = "Could not create new project" };
            }

            return AddProjectClaim(newProject.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: " + ex.Message);
            return new OperationResult { Success = false, Message = "Error: " + ex.Message };
        }
    }

    public OperationResult AddProjectClaim(int projectId, string userId)
    {
        try
        {
            var userClaim = new IdentityUserClaim<string>
            {
                UserId = userId,
                ClaimType = "ProjectAccess",
                ClaimValue = projectId.ToString()
            };

            _context.UserClaims.Add(userClaim);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: " + ex);
            return new OperationResult { Success = false, Message = "Error: " + ex.Message };
        }

        return new OperationResult { Success = true, Message = "Project was created successfully" };
    }
}

