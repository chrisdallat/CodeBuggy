using Microsoft.AspNetCore.Mvc;
using CodeBuggy.Data;
using CodeBuggy.Models;
using System.Diagnostics;
using CodeBuggy.Models.Projects;
using CodeBuggy.Helpers;
using Microsoft.AspNetCore.Identity;

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
        var tickets = _projectsModel?.GetTickets(User, projectId);

        if (tickets != null)
        {
            return View(tickets);
        }

        return RedirectToAction("Login", "Account");
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
            return Json(new { success = false, error = "Project Name must be provided" });
        }

        OperationResult result = await _projectsModel.AddNewProjectAsync(input, User);

        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProject(ProjectsModel.InputModel input)
    {
        if (string.IsNullOrWhiteSpace(input.AccessCode))
        {
            return Json(new { success = false, error = "Project Access Code must be provided" });
        }

        OperationResult result = await _projectsModel.DeleteProjectAsync(input.AccessCode, User);

        return Json(new { success = result.Success, message = result.Message });
    }
}
