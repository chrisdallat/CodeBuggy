using CodeBuggy.Controllers;
using CodeBuggy.Data;
using CodeBuggy.Helpers;
using Microsoft.AspNetCore.Mvc;
using PagedList;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace CodeBuggy.Models.Projects;

public class ProjectsListModel
{
    private readonly ILogger<ProjectsController> _logger;
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private static readonly Random Random = new Random();

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [StringLength(20, ErrorMessage = "The Project Name must be maximum 255 characters.")]
        [Display(Name = "Project name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(64, ErrorMessage = "The Access Code must be maximum 255 characters.")]
        [Display(Name = "Access code")]
        public string AccessCode { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "The Email must be maximum 255 characters.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    public ProjectsListModel(ILogger<ProjectsController> logger, AppDbContext context, UserManager<AppUser> userManager)
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

    public OperationResult InviteEmail(InputModel input, ClaimsPrincipal user)
    {
        try
        {
            var username = _userManager?.GetUserAsync(user)?.Result?.FirstName + " " + _userManager?.GetUserAsync(user)?.Result?.LastName;
            if (username == null)
            {
                return new OperationResult { Success = false, Message = "User is not authenticated" };
            }

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
            smtpClient.Port = 587;
            smtpClient.Credentials = new NetworkCredential("appcodebuggy@gmail.com", "iksw plht cdzd iqmv"); //Prob Needs some Encryptionhere
            smtpClient.EnableSsl = true;

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("appcodebuggy@gmail.com", "CodeBuggy");
            mailMessage.To.Add(input.Email);
            mailMessage.Subject = "Invite Access Code to CodeBuggy Project";
            mailMessage.IsBodyHtml = true;
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta http-equiv='X-UA-Compatible' content='IE=edge'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{
                        font-family: 'Arial', sans-serif;
                        line-height: 1.6;
                        font-size: 16px
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                    }}
                    .important-text {{
                        font-weight: bold;
                        font-size: 26px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <p>Hi <b>{input.Email}</b>,</p>
                    <p>You have been invited to join our project <b><i>{input.Name}</i></b> by <b>{username}</b>, below is your AccessCode:</p>
                    <p class='important-text' style='justify-content: center'>{input.AccessCode}</p>
                    <p>You can log in to your account and add the project by entering the project name and access code provided. If you do not have an account on CodeBuggy, you can create one on the website!</p>
                    <p>Thank You,</p>
                    <p>CodeBuggy Team</p>
                </div>
            </body>
            </html>";


            smtpClient.Send(mailMessage);

        }
        catch (Exception ex)
        {
            _logger.LogError("Error: " + ex);
            return new OperationResult { Success = false, Message = "Error: " + ex.Message };
        }

        return new OperationResult { Success = true, Message = "Invite Sent Successfully" };
    }


    // ******************************************************************************* //
    // **************************** Private Methods ********************************** // 
    // ******************************************************************************* //
    private string GenerateAccessCode()
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
}

