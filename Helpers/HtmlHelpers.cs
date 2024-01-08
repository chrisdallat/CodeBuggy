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
        thead.InnerHtml += "<tr><th>Title</th><th>Access Code</th><th>Owner</th></tr>";
        table.InnerHtml += thead;

        var tbody = new TagBuilder("tbody");
        foreach (var project in projects)
        {
            var row = new TagBuilder("tr");
            row.AddCssClass("clickable-row");
            row.Attributes.Add("data-href", $"/Project/Details/{project.Id}");

            row.InnerHtml += $"<td><a class='nav-link project-link' href='{url.Action("ProjectBoard", "Projects", new { id = project.Id })}'>{project.Name}</a></td>" +
                            $"<td><span class='blurred-text' onclick='toggleBlur(this)'><span>{project.AccessCode}</span></span>" +
                            $"<button class='gg-copy' onclick='copyText(this)'></button></td>" +
                            $"<td>{project.Owner}</td>";
            tbody.InnerHtml += row;
        }
        table.InnerHtml += tbody;

        return new HtmlString(table.ToString());
    }

    public static IHtmlContent RenderPagination(IPagedList<Project> projectList, Func<int, string> pageUrl)
    {
        var div = new TagBuilder("div");
        div.AddCssClass("pagination-box");
        div.MergeAttribute("style", "justify-content: center;");

        var mainDiv = new TagBuilder("div");
        mainDiv.AddCssClass("page-nav");

        var pages = new TagBuilder("span");
        pages.InnerHtml = $" {projectList.PageNumber} / {(projectList.PageCount == 0 ? 1 : projectList.PageCount)}";
        pages.MergeAttribute("style", "font-size:19px");

        var prevLink = new TagBuilder("a");
        prevLink.AddCssClass("page-link, gg-arrow-left-r");
        prevLink.Attributes.Add("href", projectList.HasPreviousPage ? pageUrl(projectList.PageNumber - 1) : "#");
        prevLink.MergeAttribute("style", "margin-right: 8px");

        var nextLink = new TagBuilder("a");
        nextLink.AddCssClass("page-link, gg-arrow-right-r");
        nextLink.Attributes.Add("href", projectList.HasNextPage ? pageUrl(projectList.PageNumber + 1) : "#");
        nextLink.MergeAttribute("style", "margin-left: 8px;");

        mainDiv.InnerHtml += prevLink;
        mainDiv.InnerHtml += pages;
        mainDiv.InnerHtml += nextLink;

        div.InnerHtml += mainDiv;

        return new HtmlString(div.ToString());
    }

    public static IHtmlContent RenderTickets(this IHtmlHelper htmlHelper, List<Ticket> tickets, TicketStatus status)
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