using Microsoft.AspNetCore.Mvc;

public class BurndownController : Controller
{
    public IActionResult Index()
    {
        // Your logic here
        return View("../Projects/Burndown"); // needs to be relative to Views/Home for some reason
    }
}
