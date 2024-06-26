﻿using Microsoft.AspNetCore.Mvc;
using CodeBuggy.Data;
using CodeBuggy.Models;
using System.Diagnostics;
using CodeBuggy.Models.Projects;
using CodeBuggy.Helpers;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Net.Sockets;
namespace CodeBuggy.Controllers;

public class ProjectsController : Controller
{
    private readonly ILogger<ProjectsController> _logger;
    private readonly ProjectsListModel _projectsListModel;
    private readonly ProjectBoardModel _projectBoardModel;
    private readonly NotificationModel _notificationModel;
    private readonly AppDbContext _context;
    public ProjectsController(ILogger<ProjectsController> logger, AppDbContext context, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _projectsListModel = new ProjectsListModel(_logger, context, userManager);
        _projectBoardModel = new ProjectBoardModel(_logger, context, userManager);
        _notificationModel = new NotificationModel();
        _context = context;
    }

    // ******************************************************************************* //
    // ******************************** Project List ********************************* // 
    // ******************************************************************************* //
    public IActionResult ProjectsList(int page = 1)
    {
        var projectList = _projectsListModel.GetProjectsList(page, User);

        if (projectList != null)
        {
            ViewBag.ProjectTable = HtmlHelpers.RenderProjectTable(projectList, Url);
            ViewBag.Pagination = HtmlHelpers.RenderPagination(projectList, i => Url.Action("ProjectsList", new { page = i }));
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddExistingProject(ProjectsListModel.InputModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.AccessCode))
        {
            return Json(new { success = false, message = "Project Name and Access Code must be provided" });
        }

        OperationResult result = await _projectsListModel.AddExistingProject(input, User);

        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> AddNewProject(ProjectsListModel.InputModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return Json(new { success = false, message = "Project Name must be provided" });
        }

        if (input.Name.Length > 20)
        {
            return Json(new { success = false, message = "Project Name must not exceed 20 characters" });
        }

        if (input.Name.Contains(' '))
        {
            return Json(new { success = false, message = "Project Name must not contain white spaces" });
        }

        OperationResult result = await _projectsListModel.AddNewProjectAsync(input, User);

        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProject(ProjectsListModel.InputModel input)
    {
        if (string.IsNullOrWhiteSpace(input.AccessCode))
        {
            return Json(new { success = false, message = "Project Access Code must be provided" });
        }

        OperationResult result = await _projectsListModel.DeleteProjectAsync(input.AccessCode, User);

        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public IActionResult InviteEmail(ProjectsListModel.InputModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Email))
        {
            return Json(new { success = false, message = "User could not be invited" });
        }

        OperationResult result = _projectsListModel.InviteEmail(input, User);

        return Json(new { success = result.Success, message = result.Message });
    }

    // ******************************************************************************* //
    // ******************************** Project Board ******************************** // 
    // ******************************************************************************* //

    [AcceptVerbs("GET", "POST")]
    public IActionResult ProjectBoard(int projectId, string ticket, bool userFilter)
    {
        Console.WriteLine("Chris projectId: " + projectId + " Ticket: " + ticket + " userFilter: " + userFilter);
        if (User.Identity == null || User.Identity.IsAuthenticated == false)
        {
            return RedirectToAction("Login", "Account");
        }

        if (_projectBoardModel != null && _projectBoardModel.ValidUserClaim(User, projectId))
        {
            bool result = _projectBoardModel.PopulateTickets(projectId, User, userFilter);
            if (result == false)
            {
                ViewBag.DeniedAccess = true;
                return View();
            }

            ViewBag.ProjectTitle = _projectBoardModel?.GetProjectName(projectId);
            ViewBag.DeniedAccess = false;
            ViewBag.ProjectId = projectId;
            ViewBag.Username = _projectBoardModel?.GetUsername(User);
            ViewBag.UserFilter = userFilter;

            if (ticket != null)
            {
                ViewBag.TicketId = ticket;
            }
            else
            {
                ViewBag.TicketId = null;
            }

            return View(_projectBoardModel);
        }

        ViewBag.DeniedAccess = true;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddTicket(ProjectBoardModel.InputModel input, int projectId)
    {
        OperationResult result = await _projectBoardModel.AddTicketToProject(User, input, projectId);

        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> ChangeTicketStatus(int projectId, int ticketId, string status)
    {
        if (User.Identity == null || User.Identity.IsAuthenticated == false)
        {
            return RedirectToAction("Login", "Account");
        }

        OperationResult result = await _projectBoardModel.ChangeTicketStatus(User, projectId, ticketId, status);

        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> SaveTicketChanges(int projectId, int ticketId, ProjectBoardModel.InputModel input)
    {

        if (User.Identity == null || User.Identity.IsAuthenticated == false)
        {
            return RedirectToAction("Login", "Account");
        }

        OperationResult result = await _projectBoardModel.SaveTicketChanges(User, projectId, ticketId, input);

        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> GetTicketStatus(int projectId, int ticketId)
    {
        if (User.Identity == null || User.Identity.IsAuthenticated == false)
        {
            return RedirectToAction("Login", "Account");
        }

        int ticketStatusInt = await _projectBoardModel.GetTicketStatus(User, projectId, ticketId);

        if (ticketStatusInt != -1)
        {
            return Json(new { success = true, ticketStatus = ticketStatusInt });
        }

        return Json(new { success = true, ticketStatus = ticketStatusInt });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTicket(int projectId, int ticketId)
    {
        if (User.Identity == null || User.Identity.IsAuthenticated == false)
        {
            return RedirectToAction("Login", "Account");
        }

        OperationResult result = await _projectBoardModel.DeleteTicket(User, projectId, ticketId);

        return Json(new { success = result.Success, message = result.Message });
    }


    [HttpPost]
    public List<Notification> GetNotifications(int projectId)
    {
        List<Notification> data = _notificationModel.GetNotificationData(_context, projectId);

        return data;
    }


    [HttpPost]
    public async Task<IActionResult> AddCommentToTicket(int projectId, int ticketId, string comment)
    {
        if (User.Identity == null || User.Identity.IsAuthenticated == false)
        {
            return RedirectToAction("Login", "Account");
        }

        OperationResult result = await _projectBoardModel.AddCommentToTicket(User, projectId, ticketId, comment);

        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public IActionResult GetComments(int projectId, int ticketId)
    {
        if (User.Identity == null || User.Identity.IsAuthenticated == false)
        {
            return RedirectToAction("Login", "Account");
        }

        var comments = new List<Comment>();

        OperationResult result = _projectBoardModel.GetComments(User, projectId, ticketId, ref comments);

        return Json(new { success = result.Success, message = result.Message, commentsData = comments });
    }

    [HttpPost]
    public async Task<IActionResult> GetTicketInfo(int projectId, int searchTicketId)
    {
        if (User.Identity == null || User.Identity.IsAuthenticated == false)
        {
            return RedirectToAction("Login", "Account");
        }

        var foundTicket = await _projectBoardModel?.GetTicketInfo(User, projectId, searchTicketId);

        if (foundTicket != null)
        {
            var assigneeUser = "false";
            var username = _projectBoardModel?.GetUsername(User);
            if (username != null && username == foundTicket.Assignee)
            {
                assigneeUser = "true";
            }
            return Json(new { success = true, ticketJson = JsonConvert.SerializeObject(foundTicket), assignedToUser = assigneeUser.ToLower() });
        }

        return Json(new { success = false, message = "Ticket not found" }); ;
    }

    [HttpPost]
    public async Task<IActionResult> AssignTicketToUser(int projectId, int ticketId)
    {
        if (User.Identity == null || User.Identity.IsAuthenticated == false)
        {
            return RedirectToAction("Login", "Account");
        }

        OperationResult result = await _projectBoardModel.AssignTicketToUser(User, projectId, ticketId);
        Console.WriteLine(result.Message);
        return Json(new { success = result.Success, message = result.Message });
    }
        // ******************************************************************************* //
        // ************************************ General ********************************** // 
        // ******************************************************************************* //
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public List<Ticket> Search(int projectId, string query)
    {
        _logger.LogInformation("ProjectsController/Search");
        List<Ticket> data = _projectBoardModel.GetSearchResults(User, projectId, query);

        return data;
    }
}

