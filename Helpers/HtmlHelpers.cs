using Microsoft.AspNetCore.Html;
using PagedList;
using CodeBuggy.Data;
using System.Web.Mvc;
using IHtmlHelper = Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace CodeBuggy.Helpers;

public static class HtmlHelpers
{
    public static IHtmlContent RenderProjectTable(IEnumerable<Project> projects, IUrlHelper url)
    {
        var table = new TagBuilder("table");
        table.AddCssClass("content-table");
        
        var thead = new TagBuilder("thead");
        thead.InnerHtml += "<tr><th>ID</th><th>Title</th><th>Description</th></tr>";
        table.InnerHtml += thead;

        var tbody = new TagBuilder("tbody");
        foreach (var project in projects)
        {
            var row = new TagBuilder("tr");
            row.AddCssClass("clickable-row");
            row.Attributes.Add("data-href", $"/Project/Details/{project.Id}");
            row.InnerHtml += $"<td>{project.Id}</td><td><a class='nav-link project-link' href='{url.Action("ProjectBoard", "Projects", new { id = project.Id })}'>{project.Name}</a></td><td>{project.AccessCode}</td>";
            tbody.InnerHtml += row;
        }
        table.InnerHtml += tbody;

        return new HtmlString(table.ToString());
    }

    public static IHtmlContent RenderPagination(IPagedList<Project> projectList, Func<int, string> pageUrl)
    {
        var div = new TagBuilder("div");
        div.AddCssClass("pagination-box");

        var form = new TagBuilder("form");
        form.Attributes.Add("action", pageUrl(1)); // Default action to the first page
        form.Attributes.Add("method", "get");

        var input = new TagBuilder("input");
        input.Attributes.Add("type", "number");
        input.Attributes.Add("min", "1");
        input.Attributes.Add("max", projectList.PageCount.ToString());
        input.Attributes.Add("value", projectList.PageNumber.ToString());
        input.AddCssClass("page-input");
        input.Attributes.Add("name", "page");

        var prevLink = new TagBuilder("a");
        prevLink.AddCssClass("page-link, gg-arrow-left-r");
        prevLink.Attributes.Add("href", projectList.HasPreviousPage ? pageUrl(projectList.PageNumber - 1) : "#");

        var nextLink = new TagBuilder("a");
        nextLink.AddCssClass("page-link, gg-arrow-right-r");
        nextLink.Attributes.Add("href", projectList.HasNextPage ? pageUrl(projectList.PageNumber + 1) : "#");

        var maxPageSpan = new TagBuilder("span");
        maxPageSpan.AddCssClass("max-page");
        maxPageSpan.InnerHtml += $" / {projectList.PageCount}";

        form.InnerHtml += prevLink;
        form.InnerHtml += input;
        form.InnerHtml += maxPageSpan;
        form.InnerHtml += nextLink;

        div.InnerHtml += form;

        var script = new TagBuilder("script");
        script.InnerHtml += @"
        function validatePageInput(input) {
            var maxPage = parseInt(input.getAttribute('max'));
            var enteredValue = parseInt(input.value);
    
            if (isNaN(enteredValue) || enteredValue < 1) {
                input.value = 1;
            } else if (enteredValue > maxPage) {
                input.value = maxPage;
            }
        }
        ";

        div.InnerHtml += script;

        return new HtmlString(div.ToString());
    }

    public static async Task<IHtmlContent> RenderTicketsAsync(this IHtmlHelper htmlHelper, List<Ticket> tickets, TicketStatus status)
    {
        var ticketByStatus = tickets.Where(t => t.Status == status);
        var result = new StringBuilder();

        foreach (var ticket in ticketByStatus)
        {
            switch (ticket.Priority)
            {
                case TicketPriority.Urgent:
                    result.Append($"<div class='ticket' style='background-color: #D98695;'><h4>{ticket.Title}</h4><p>{ticket.Priority}</p></div>");
                    break;

                case TicketPriority.High:
                    result.Append($"<div class='ticket' style='background-color: #BBB477;'><h4>{ticket.Title}</h4><p>{ticket.Priority}</p></div>");
                    break;

                case TicketPriority.Medium:
                    result.Append($"<div class='ticket' style='background-color: #56887D;'><h4>{ticket.Title}</h4><p>{ticket.Priority}</p></div>");
                    break;

                case TicketPriority.Low:
                    result.Append($"<div class='ticket' style='background-color: #A6A6A6;'><h4>{ticket.Title}</h4><p>{ticket.Priority}</p></div>");
                    break;

                default:
                    result.Append($"<div class='ticket'><h4>{ticket.Title}</h4></div>");
                    break;

            }
        }

        return new HtmlString(result.ToString());
    } 

}