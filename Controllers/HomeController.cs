using CodeBuggy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using CodeBuggy.Data;
using Microsoft.EntityFrameworkCore.Design;
using PagedList;

namespace CodeBuggy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        
        public IActionResult Index(int? page)
        {
            int pageSize = 3;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var items = _context.Projects.Select(e => new Project
                {
                    Id = e.Id,
                    Name = e.Name,
                    AccessCode = e.AccessCode
                }).ToPagedList(page ?? 1, pageSize);
                ViewBag.ProjectList = items;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View( );
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
