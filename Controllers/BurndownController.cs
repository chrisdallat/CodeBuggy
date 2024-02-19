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
    private readonly BurndownModel _burndownModel;
    private readonly AppDbContext _context;

    // Private field to store projectId
    private int _projectId;

    public BurndownController(ILogger<BurndownController> logger, AppDbContext context, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _burndownModel = new BurndownModel();
        _context = context;
    }

    public IActionResult Burndown(int projectId)
    {
        _logger.LogInformation("Chris " + projectId);

        ViewBag.projectId = projectId;

        return View(_burndownModel);
    }

    [HttpPost]
    public List<DailyTicketCounts> GetDailyTicketCounts(int projectId)
    {
        _logger.LogInformation("Using stored projectId: " + projectId);
        List<DailyTicketCounts> data = _burndownModel.GetBurndownData(_context, projectId);
        _logger.LogInformation("DATA in BurndownController: " + JsonConvert.SerializeObject(data));

        return data;
    }
}
