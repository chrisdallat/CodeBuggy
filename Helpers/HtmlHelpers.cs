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

            row.InnerHtml += $"<td><a class='nav-link project-link' href='{url.Action("ProjectBoard", "Projects", new { projectId = project.Id })}'>{project.Name}</a></td>" +
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
            ticketHtml.AddCssClass("card");
            ticketHtml.MergeAttribute("draggable", "true");
            ticketHtml.MergeAttribute("ondragstart", $"drag(event, this)");

            var ticketTitle = new TagBuilder("h3");
            ticketTitle.AddCssClass("card__title");
            ticketTitle.InnerHtml = $"{ticket.Title} <span class='priority'>{ticket.Priority}</span>";

            var ticketDescription = new TagBuilder("p");
            ticketDescription.AddCssClass("card__content");
            ticketDescription.InnerHtml = ticket.Description;

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

            ticketHtml.InnerHtml += ticketTitle;
            ticketHtml.InnerHtml += ticketDescription;
            ticketHtml.InnerHtml += ticketCreateDate;
            ticketHtml.InnerHtml += cardArrow;

            result.Append(ticketHtml.ToString());
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

    private static string AddTicketStyle()
    {
        return "<div class=\"card__arrow\">\r\n        <svg xmlns=\"http://www.w3.org/2000/svg\" fill=\"none\" viewBox=\"0 0 24 24\" height=\"15\" width=\"15\">\r\n            <path fill=\"#fff\" d=\"M13.4697 17.9697C13.1768 18.2626 13.1768 18.7374 13.4697 19.0303C13.7626 19.3232 14.2374 19.3232 14.5303 19.0303L20.3232 13.2374C21.0066 12.554 21.0066 11.446 20.3232 10.7626L14.5303 4.96967C14.2374 4.67678 13.7626 4.67678 13.4697 4.96967C13.1768 5.26256 13.1768 5.73744 13.4697 6.03033L18.6893 11.25H4C3.58579 11.25 3.25 11.5858 3.25 12C3.25 12.4142 3.58579 12.75 4 12.75H18.6893L13.4697 17.9697Z\"></path>\r\n        </svg>\r\n    </div>";
    }

}