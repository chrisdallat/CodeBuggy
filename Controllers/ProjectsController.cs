using Microsoft.AspNetCore.Mvc;
using CodeBuggy.Data;
using PagedList;
using CodeBuggy.Helpers;
using Microsoft.Build.ObjectModelRemoting;
using CodeBuggy.Models;
using System.Diagnostics;

namespace CodeBuggy.Controllers;

public class ProjectsController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;
    private int _lastPage;

    public ProjectsController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
        _lastPage = 1;
    }

    public IActionResult ProjectsList(int page = 1)
    {
        int pageSize = 1;

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var projectList = _context.Projects.Select(e => new Project
            {
                Id = e.Id,
                Name = e.Name,
                AccessCode = e.AccessCode
            }).ToPagedList(page, pageSize);

            var viewModel = new AppUser
            {
                ProjectList = projectList
            };

            ViewBag.ProjectTable = HtmlHelpers.RenderProjectTable(projectList);
            ViewBag.Pagination = HtmlHelpers.RenderPagination(projectList, i => Url.Action("ProjectsList", new { page = i }));

            // FIXME: If user puts a higher page than the actual count then it loads an empty table
            if (page > projectList.PageCount)
            {
                _logger.LogInformation("Im here 1");
                return RedirectToAction("Error");
            }

            return View(viewModel);
        }

        return View();
    }

    public IActionResult ProjectBoard()
    {
        List<Ticket> tickets = new List<Ticket>
        {
           new Ticket { Id = 1, Title = "Task 1", Description = "Description 1", Priority = TicketPriority.None, Status = TicketStatus.ToDo },
            new Ticket { Id = 2, Title = "Task 2", Description = "Description 2", Priority = TicketPriority.Low, Status = TicketStatus.InProgress },
            new Ticket { Id = 3, Title = "Task 3", Description = "Description 3", Priority = TicketPriority.Medium, Status = TicketStatus.Done },
            new Ticket { Id = 4, Title = "Task 4", Description = "Description 4", Priority = TicketPriority.High, Status = TicketStatus.Done },
            new Ticket { Id = 5, Title = "Task 5", Description = "Description 5", Priority = TicketPriority.Urgent, Status = TicketStatus.ToDo },
            new Ticket { Id = 6, Title = "Task 6", Description = "Description 6", Priority = TicketPriority.None, Status = TicketStatus.InProgress },
            new Ticket { Id = 7, Title = "Task 7", Description = "Description 7", Priority = TicketPriority.Low, Status = TicketStatus.ToDo },
            new Ticket { Id = 8, Title = "Task 8", Description = "Description 8", Priority = TicketPriority.Medium, Status = TicketStatus.Done },
            new Ticket { Id = 9, Title = "Task 9", Description = "Description 9", Priority = TicketPriority.High, Status = TicketStatus.InProgress },
            new Ticket { Id = 10, Title = "Task 10", Description = "Description 10", Priority = TicketPriority.Urgent, Status = TicketStatus.InProgress },
            new Ticket { Id = 11, Title = "Task 11", Description = "Description 11", Priority = TicketPriority.None, Status = TicketStatus.ToDo },
            new Ticket { Id = 12, Title = "Task 12", Description = "Description 12", Priority = TicketPriority.Low, Status = TicketStatus.Done },
            new Ticket { Id = 13, Title = "Task 13", Description = "Description 13", Priority = TicketPriority.Medium, Status = TicketStatus.Done },
            new Ticket { Id = 14, Title = "Task 14", Description = "Description 14", Priority = TicketPriority.High, Status = TicketStatus.ToDo },
            new Ticket { Id = 15, Title = "Task 15", Description = "Description 15", Priority = TicketPriority.Urgent, Status = TicketStatus.InProgress },
        };
        return View(tickets);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
