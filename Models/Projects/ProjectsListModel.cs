﻿using CodeBuggy.Controllers;
using CodeBuggy.Data;
using CodeBuggy.Helpers;
using Microsoft.AspNetCore.Mvc;
using PagedList;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeBuggy.Models.Projects;

public class ProjectsModel
{
    private readonly ILogger<ProjectsController> _logger;
    private readonly AppDbContext _context;
    private static readonly Random Random = new Random();
    private readonly UserManager<AppUser> _userManager;

    public List<Ticket> Tickets { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(255, ErrorMessage = "The Project Name must be maximum 255 characters.")]
        [Display(Name = "Project name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(255, ErrorMessage = "The Access Code must be maximum 255 characters.")]
        [Display(Name = "Access code")]
        public string AccessCode { get; set; } = string.Empty;

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
            _logger.LogError("Unable to load projects: " + ex);
            return null;
        }
    }

    public List<Ticket>? GetTickets(int projectId)
    {
        try
        {
            var project = _context.Projects.FirstOrDefault(p => p.Id == projectId);

            if (project == null)
            {
                return null;
            }

            // Retrieve the list of tickets for the specified project
            var tickets = _context.Tickets
                .Where(t => project.TicketsId.Contains(t.Id))
                .ToList();

            return tickets;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving tickets: " + ex);
            return null;
        }
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

    public bool ValidUserClaim(ClaimsPrincipal user, int projectId)
    {
        var userId = _userManager?.GetUserId(user);
        if (userId == null )
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

    public async Task<OperationResult> AddExistingProject(InputModel input, ClaimsPrincipal user)
    {
        if (user.Identity == null || user.Identity.IsAuthenticated == false)
        {
            return new OperationResult { Success = false, Message = "User is not authenticated" };
        }

        var existingProject = _context.Projects.FirstOrDefault(p => p.Name == input.Name && p.AccessCode == input.AccessCode);

        if (existingProject == null)
        {
            return new OperationResult { Success = false, Message = "Unable to locate project with this name" };
        }

        var userId = _userManager?.GetUserId(user);
        if (userId == null || existingProject?.Id == null || existingProject.Id == 0)
        {
            return new OperationResult { Success = false, Message = "Unable to join existing project" };
        }

        var projectClaimed = _context.UserClaims.FirstOrDefault(p => p.ClaimValue == existingProject.Id.ToString() && p.ClaimType == "ProjectAccess" && p.UserId == userId);
        if (projectClaimed != null)
        {
            return new OperationResult { Success = false, Message = "Access to project previously granted" };
        }

        bool result = AddProjectClaim(existingProject.Id, userId).Success;
        return new OperationResult { Success = result, Message = "Project added" };
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
                    return new OperationResult { Success = false, Message = "Project with identical name exists" };
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
                return new OperationResult { Success = false, Message = "Unable to create new project" };
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

    public async Task<OperationResult> DeleteProjectAsync(string accessCode, ClaimsPrincipal user)
    {
        try
        {
            var projectToDelete = await _context.Projects.FirstOrDefaultAsync(p => p.AccessCode == accessCode);

            var userId = _userManager?.GetUserId(user);

            if (projectToDelete == null || userId == null)
            {
                return new OperationResult { Success = false, Message = "Unable to delete project" };
            }

            if (projectToDelete.OwnerId != userId)
            {
                var haveAccessToProject = await _context.UserClaims.FirstOrDefaultAsync(p => p.ClaimValue == projectToDelete.Id.ToString() && p.ClaimType == "ProjectAccess" && p.UserId == userId);
                if (haveAccessToProject == null)
                {
                    return new OperationResult { Success = false, Message = "Unable to delete project" };
                }

                _context.UserClaims.Remove(haveAccessToProject);
                await _context.SaveChangesAsync();

                return new OperationResult { Success = true, Message = "Project deleted" };
            }

            var projectClaims = _context.UserClaims.Where(uc => uc.ClaimValue == projectToDelete.Id.ToString() && uc.ClaimType == "ProjectAccess").ToList();
            foreach (var claim in projectClaims)
            {
                _context.UserClaims.Remove(claim);
            }

            _context.Projects.Remove(projectToDelete);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: " + ex);
            return new OperationResult { Success = false, Message = "Error: " + ex.Message };
        }

        return new OperationResult { Success = true, Message = "Project deleted" };
    }
}

