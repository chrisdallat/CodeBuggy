using Microsoft.AspNetCore.Mvc;
using CodeBuggy.Data;
using CodeBuggy.Models.Projects;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

public class BurndownController : Controller
{
    private readonly ILogger<BurndownController> _logger;
    private readonly BurndownModel _burndownModel;
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    // Private field to store projectId
    private int _projectId;

    public BurndownController(ILogger<BurndownController> logger, AppDbContext context, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
        _context = context;
        _burndownModel = new BurndownModel();
    }

    public IActionResult Burndown(int projectId)
    {
        if (User.Identity == null || User.Identity.IsAuthenticated == false)
        {
            return RedirectToAction("Login", "Account");
        }

        if (_burndownModel.ValidUserClaim(_context, _userManager, User, projectId) == false)
        {
            ViewBag.DeniedAccess = true;
            return View();
        }

        ViewBag.DeniedAccess = false;
        ViewBag.projectId = projectId;

        return View(_burndownModel);
    }

    [HttpPost]
    public List<DailyTicketCounts> GetDailyTicketCounts(int projectId)
    {
        List<DailyTicketCounts> data = _burndownModel.GetBurndownData(_context, projectId);
        _logger.LogInformation("DATA in BurndownController: " + JsonConvert.SerializeObject(data));

        return data;
    }
}
