using Microsoft.AspNetCore.Html;
using PagedList;
using CodeBuggy.Data;
using System.Web.Mvc;
using IHtmlHelper = Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using CodeBuggy.Models.Projects;

namespace CodeBuggy.Helpers;

public static class HtmlHelpers
{
    public class TicketInfo
    {
        public string TicketJson { get; set; } = string.Empty;
        public string AssignedToUser { get; set; } = string.Empty;
    }
    public static IHtmlContent RenderProjectTable(IEnumerable<Project> projects, IUrlHelper url)
    {
        var table = new TagBuilder("table");
        table.AddCssClass("content-table");

        var thead = new TagBuilder("thead");
        thead.InnerHtml += "<tr><th>Title</th><th>Access Code</th><th></th><th></th><th style='width: 200px;'>Owner</th></tr>";
        table.InnerHtml += thead;

        var tbody = new TagBuilder("tbody");
        foreach (var project in projects)
        {
            var row = new TagBuilder("tr");
            row.AddCssClass("clickable-row");
            row.Attributes.Add("data-href", $"/Project/Details/{project.Id}");

            row.InnerHtml += $"<td><a class='nav-link project-link' href='{url.Action("ProjectBoard", "Projects", new { projectId = project.Id })}'>{project.Name}</a></td>" +
                            $"<td style='width: 600px;'><span class='blurred-text' onclick='toggleBlur(this)'><span>{project.AccessCode}</span></span>" +
                            $"<button class='gg-copy' onclick='copyText(this, \"{project.AccessCode}\")'></button>" +
                            $"<span style='display: none; float: right; margin-right: 20px'>Copied!</span></td>" +
                            $"<td><button class='gg-comment' onclick='toggleInviteEmailPopup(\"{project.AccessCode}\", \"{project.Name}\")'></button></td>" +
                            $"<td><button class='gg-trash' onclick='toggleDeletePopup()'></button></td>" +
                            $"<td style='width: 150px;'>{project.Owner}</td>";
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

    public static IHtmlContent RenderTickets(this IHtmlHelper htmlHelper, List<Ticket> tickets, TicketStatus status, string username)
    {
        var ticketByStatus = tickets.Where(t => t.Status == status);
        var result = new StringBuilder();

        foreach (var ticket in ticketByStatus)
        {

            var assignedToUser = "false";
            if (username != null && username == ticket.Assignee)
            {
                assignedToUser = "true";
            }
            var color = GetPriorityColor(ticket.Priority);
            var ticketHtml = new TagBuilder("div");
            ticketHtml.AddCssClass("card");
            ticketHtml.MergeAttribute("draggable", "true");
            ticketHtml.MergeAttribute("ondragstart", $"drag(event, this)");
            string ticketJson = JsonConvert.SerializeObject(ticket);
            ticketHtml.MergeAttribute("onclick", $"showTicket({ticketJson}, {assignedToUser.ToString().ToLower()})");

            var ticketId = new TagBuilder("div");
            ticketId.GenerateId("draggedTicketId");
            ticketId.MergeAttribute("style", "display: none");
            ticketId.InnerHtml = ticket.Id.ToString();

            var ticketStringId = new TagBuilder("h3");
            ticketStringId.AddCssClass("card__title");
            ticketStringId.InnerHtml = $"{ticket.StringId} <span class='priority' style='color: {color}'>{ticket.Priority}</span>";

            var ticketTitle = new TagBuilder("p");
            ticketTitle.AddCssClass("card__content");
            ticketTitle.InnerHtml = ticket.Title;

            var ticketCreateDate = new TagBuilder("div");
            ticketCreateDate.AddCssClass("card__date");
            ticketCreateDate.InnerHtml = ticket.CreationDate.ToString("MMMM dd, yyyy");

            var cardArrow = new TagBuilder("div");
            cardArrow.AddCssClass("card__arrow");

            var svg = new TagBuilder("svg");
            svg.MergeAttribute("xmlns", "http://www.w3.org/2000/svg");
            svg.MergeAttribute("fill", "none");
            svg.MergeAttribute("viewBox", "0 0 24 24");
            svg.MergeAttribute("height", "15");
            svg.MergeAttribute("width", "15");

            var path = new TagBuilder("path");
            path.MergeAttribute("fill", "#fff");
            path.MergeAttribute("d", "M13.4697 17.9697C13.1768 18.2626 13.1768 18.7374 13.4697 19.0303C13.7626 19.3232 14.2374 19.3232 14.5303 19.0303L20.3232 13.2374C21.0066 12.554 21.0066 11.446 20.3232 10.7626L14.5303 4.96967C14.2374 4.67678 13.7626 4.67678 13.4697 4.96967C13.1768 5.26256 13.1768 5.73744 13.4697 6.03033L18.6893 11.25H4C3.58579 11.25 3.25 11.5858 3.25 12C3.25 12.4142 3.58579 12.75 4 12.75H18.6893L13.4697 17.9697Z");

            svg.InnerHtml += path;

            cardArrow.InnerHtml += svg;

            ticketHtml.InnerHtml += ticketId;
            ticketHtml.InnerHtml += ticketStringId;
            ticketHtml.InnerHtml += ticketTitle;
            ticketHtml.InnerHtml += ticketCreateDate;
            ticketHtml.InnerHtml += cardArrow;

            result.Append(ticketHtml.ToString());
        }

        return new HtmlString(result.ToString());
    }

    public static TicketInfo? GetTicketInfo(this IHtmlHelper htmlHelper, List<Ticket> tickets, string ticketStringId, string username)
    {
        var ticket = tickets.FirstOrDefault(t => t.StringId == ticketStringId);

        if (ticket == null)
        {
            return null;
        }

        var result = new StringBuilder();

        var assignedToUser = "false";
        if (username != null && username == ticket.Assignee)
        {
            assignedToUser = "true";
        }
        string ticketJson = JsonConvert.SerializeObject(ticket);
        if (ticketJson == null)
        {
            return null;
        }


        return new TicketInfo
        {
            TicketJson = ticketJson,
            AssignedToUser = assignedToUser.ToLower()
        };
    }


    private static string GetPriorityColor(TicketPriority priority)
    {
        switch (priority)
        {
            case TicketPriority.Urgent:
                return "#FF3B02";
            case TicketPriority.High:
                return "#FFBE02";
            case TicketPriority.Medium:
                return "#F3E102";
            case TicketPriority.Low:
                return "#58DC22";
            case TicketPriority.None:
                return "#01CCF4";
            default:
                return string.Empty;
        }
    }

}