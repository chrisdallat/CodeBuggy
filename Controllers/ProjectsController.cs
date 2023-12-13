using Microsoft.AspNetCore.Mvc;
using CodeBuggy.Data;
using PagedList;
using CodeBuggy.Helpers;
using CodeBuggy.Models;
using System.Diagnostics;
using CodeBuggy.Models.Projects;
using System.Dynamic;

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

    public IActionResult ProjectBoard()
    {
        List<Ticket> tickets = _projectsModel.getTickets(User);

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
}
