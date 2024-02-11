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
using Newtonsoft.Json;

public class BurndownController : Controller
{
    private readonly ILogger<BurndownController> _logger;
    private readonly BurndownModel _burndown;
    private readonly AppDbContext _context;

    // Private field to store projectId
    private int _projectId;

    public BurndownController(ILogger<BurndownController> logger, AppDbContext context, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _burndown = new BurndownModel();
        _context = context;
    }

    public IActionResult Index(int projectId)
    {
        _logger.LogInformation("Chris " + projectId);

        _projectId = projectId;

        return View("../Projects/Burndown"); // needs to be relative to Views/Home for some reason
    }

    [HttpPost]
    public List<DailyTicketCounts> GetDailyTicketCounts()
    {
        _logger.LogInformation("Using stored projectId: " + _projectId);
        List<DailyTicketCounts> data = _burndown.GetBurndownData(_context, 17);
        _logger.LogInformation("DATA in BurndownController: " + JsonConvert.SerializeObject(data));

        return data;
    }
}
