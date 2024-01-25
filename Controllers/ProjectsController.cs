using Microsoft.AspNetCore.Mvc;
using CodeBuggy.Data;
using CodeBuggy.Models;
using System.Diagnostics;
using CodeBuggy.Models.Projects;
using CodeBuggy.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CodeBuggy.Controllers;

public class ProjectsController : Controller
{
    private readonly ILogger<ProjectsController> _logger;
    private readonly ProjectsModel _projectsModel;
    public ProjectsController(ILogger<ProjectsController> logger, AppDbContext context, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _projectsModel = new ProjectsModel(_logger, context, userManager);
    }

    public IActionResult ProjectsList(int page = 1)
    {
        var projectList = _projectsModel.GetProjectsList(page, User);

        if (projectList != null)
        {
            ViewBag.ProjectTable = HtmlHelpers.RenderProjectTable(projectList, Url);
            ViewBag.Pagination = HtmlHelpers.RenderPagination(projectList, i => Url.Action("ProjectsList", new { page = i }));
        }

        return View();
    }

    public IActionResult ProjectBoard(int projectId)
    {
        if (User.Identity == null || User.Identity.IsAuthenticated == false)
        {
            return RedirectToAction("Login", "Account");
        }

        if (_projectsModel.ValidUserClaim(User, projectId))
        {
            _projectsModel.Tickets = _projectsModel?.GetTickets(projectId);
            ViewBag.ProjectTitle = _projectsModel?.GetProjectName(projectId);
            ViewBag.DeniedAccess = false;
            return View(_projectsModel);
        }

        ViewBag.DeniedAccess = true;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public ActionResult NewProject()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddExistingProject(ProjectsModel.InputModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.AccessCode))
        {
            return Json(new { success = false, message = "Project Name and Access Code must be provided" });
        }

        OperationResult result = await _projectsModel.AddExistingProject(input, User);

        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> AddNewProject(ProjectsModel.InputModel input)
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

        OperationResult result = await _projectsModel.AddNewProjectAsync(input, User);

        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProject(ProjectsModel.InputModel input)
    {
        if (string.IsNullOrWhiteSpace(input.AccessCode))
        {
            return Json(new { success = false, message = "Project Access Code must be provided" });
        }

        OperationResult result = await _projectsModel.DeleteProjectAsync(input.AccessCode, User);

        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> AddTicket(ProjectsModel.InputModel input, int projectId)
    {

        if (string.IsNullOrWhiteSpace(input.TicketTitle))
        {
            return Json(new { success = false, message = "Ticket title must be provided" });
        }

        OperationResult result = await _projectsModel.AddTicketToProject(User, input, projectId);

        return Json(new { success = result.Success, message = result.Message });
    }
}
