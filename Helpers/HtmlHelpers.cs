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
        thead.InnerHtml += "<tr><th>Title</th><th>Access Code</th><th>Owner</th><th></th></tr>";
        table.InnerHtml += thead;

        var tbody = new TagBuilder("tbody");
        foreach (var project in projects)
        {
            var row = new TagBuilder("tr");
            row.AddCssClass("clickable-row");
            row.Attributes.Add("data-href", $"/Project/Details/{project.Id}");

            row.InnerHtml += $"<td><a class='nav-link project-link' href='{url.Action("ProjectBoard", "Projects", new { id = project.Id })}'>{project.Name}</a></td>" +
                            $"<td style='width: 600px;'><span class='blurred-text' onclick='toggleBlur(this)'><span>{project.AccessCode}</span></span>" +
                            $"<button class='gg-copy' onclick='copyText(this, \"{project.AccessCode}\")'></button>" +
                            $"<span style='display: none; float: right; margin-right: 20px'>Copied!</span></td>" +
                            $"<td>{project.Owner}</td>" +
                            $"<td><button class='gg-trash' onclick='toggleDeletePopup(this, \"{project.AccessCode}\")'></button></td>";
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
            var ticketHtml = new TagBuilder("div");
            ticketHtml.AddCssClass("ticket");
            ticketHtml.MergeAttribute("draggable", "true");

            var priorityColor = GetPriorityColor(ticket.Priority);

            ticketHtml.InnerHtml = ($"<h4>{ticket.Title}</h4>");
            ticketHtml.InnerHtml = ($"<p>{ticket.Priority}</p>");

            ticketHtml.MergeAttribute("ondragstart", $"drag(event, this)");
            result.Append(ticketHtml.ToString(TagRenderMode.StartTag));
            result.Append($"<div class='ticket' style='background-color: {priorityColor};'><h4>{ticket.Title}</h4><p>{ticket.Priority}</p></div>");
            result.Append("</div>");
        }

        return new HtmlString(result.ToString());
    }

    private static string GetPriorityColor(TicketPriority priority)
    {
        switch (priority)
        {
            case TicketPriority.Urgent:
                return "#D98695";
            case TicketPriority.High:
                return "#BBB477";
            case TicketPriority.Medium:
                return "#56887D";
            case TicketPriority.Low:
                return "#A6A6A6";
            default:
                return string.Empty;
        }
    }

}