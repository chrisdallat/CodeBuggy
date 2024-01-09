using Microsoft.AspNetCore.Mvc;
using CodeBuggy.Data;
using CodeBuggy.Models;
using System.Diagnostics;
using CodeBuggy.Models.Projects;
using CodeBuggy.Helpers;
using Field = CodeBuggy.Helpers.Popup.Field;
using System.Security.Policy;
using System;
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
        //Popup NewProjectPopup = new Popup
        //{
        //    Title = "New Project",
        //    Fields = new Field[]
        //    {
        //        new Field { Label = "Project Name", Type = "Input", Value = "" },
        //        new Field { Label = "Project Name1", Type = "Input", Value = "" },
        //        new Field { Label = "Project Name2", Type = "Input", Value = "" }
        //    }
        //};

        //NewProjectPopup.Create();

        //string html = NewProjectPopup.GetPopupHtml();

        //if (html != null)
        //{
        //    _logger.LogInformation(html);
        //    return Content(html, "text/html");
        //}
        //_logger.LogInformation("html is null");
        //return Content("Popup not shown", "text/plain");

        //if(User.Identity && )

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddExistingProject(ProjectsModel.InputModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.AccessCode))
        {
            return Json(new { success = false, message = "Project name or access code cannot be empty." });
        }

        OperationResult result = await _projectsModel.AddExistingProject(input, User);

        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> AddNewProject(ProjectsModel.InputModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return Json(new { success = false, error = "Project name cannot be empty." });
        }

        OperationResult result = await _projectsModel.AddNewProjectAsync(input, User);

        return Json(new { success = result.Success, message = result.Message });
    }
}
