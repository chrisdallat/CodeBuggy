using Microsoft.AspNetCore.Mvc;
using CodeBuggy.Data;
using CodeBuggy.Models;
using System.Diagnostics;
using CodeBuggy.Models.Projects;
using CodeBuggy.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using CodeBuggy.Migrations;

public class BurndownController : Controller
{
    private readonly ILogger<BurndownController> _logger;
    private readonly BurndownModel _burndown;
    public BurndownController(ILogger<BurndownController> logger, AppDbContext context, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _burndown = new BurndownModel();
    }
    public IActionResult Index(int projectId)
    {
        _logger.LogInformation("Chris " + projectId);
        // Your logic here
        return View("../Projects/Burndown"); // needs to be relative to Views/Home for some reason
    }
}
