using Microsoft.AspNetCore.Mvc;
using CodeBuggy.Data;
using CodeBuggy.Models;
using System.Diagnostics;
using CodeBuggy.Models.Projects;
using CodeBuggy.Helpers;
using Field = CodeBuggy.Helpers.Popup.Field;

namespace CodeBuggy.Controllers;

public class ProjectsController : Controller
{
    private readonly ILogger<ProjectsController> _logger;
    private readonly ProjectsModel _projectsModel;
    public ProjectsController(ILogger<ProjectsController> logger, AppDbContext context)
    {
        _logger = logger;
        _projectsModel = new ProjectsModel(_logger, context);
    }

    public IActionResult ProjectsList(int page = 1)
    {
        var viewModel = _projectsModel.GetProjectsList(page, ViewBag, User, Url);

        if (viewModel != null)
        {
            return View(viewModel);
        }

        return View();
    }

    public IActionResult ProjectBoard(int projectId)
    {
        List<Ticket> tickets = _projectsModel.getTickets(User, projectId);

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
}
