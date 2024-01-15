using System.Web.Mvc;

namespace CodeBuggy.Helpers;
public class Popup
{
    public string? Title { get; set; }

    public string? PopupHtml { get; set; }

    public string CreatePopup(string popupid, string title, string[] elements)
    {
        var popupOverlay = new TagBuilder("div");
        popupOverlay.MergeAttribute("id", popupid);
        popupOverlay.AddCssClass("overlay");

        var popupDiv = new TagBuilder("div");
        popupDiv.AddCssClass("popup");

        var popupTop = new TagBuilder("div");
        popupTop.AddCssClass("popup-top");

        var backButtonSpan = new TagBuilder("span");
        backButtonSpan.GenerateId("goBackButton");
        backButtonSpan.AddCssClass("gg-arrow-left-r");
        backButtonSpan.MergeAttribute("style", "position:fixed; margin-top: 8px; display: none");
        backButtonSpan.MergeAttribute("onclick", "goBack()");

        var closeButtonSpan = new TagBuilder("span");
        closeButtonSpan.AddCssClass("popup-close-button");
        closeButtonSpan.AddCssClass("gg-close-r");
        closeButtonSpan.MergeAttribute("onclick", "togglePopup()");

        var popupTitle = new TagBuilder("h2");
        popupTitle.MergeAttribute("style", "text-align: center;");
        popupTitle.InnerHtml = title;

        popupTop.InnerHtml = backButtonSpan.ToString() + popupTitle.ToString() + closeButtonSpan.ToString();
        
        popupDiv.InnerHtml += popupTop.ToString();

        foreach (var element in elements)
        {
            popupDiv.InnerHtml += element.ToString();
        }

        popupOverlay.InnerHtml = popupDiv.ToString();


        //var scrollableContentDiv = new TagBuilder("div");
        //foreach (var field in Fields) 
        //{
        //    var container = new TagBuilder("div");
        //    container.AddCssClass("containerBox");

        //    var label = new TagBuilder("label");
        //    label.AddCssClass("left-div");
        //    label.InnerHtml = field.Label;

        //    switch (field.Type)
        //    {
        //        case "Input":
        //            var input = new TagBuilder("Input");
        //            input.AddCssClass("right-div");
        //            container.InnerHtml = label.ToString() + input.ToString(); 
        //            break;

        //        default:
        //            break;
        //    }

        //    scrollableContentDiv.InnerHtml += container.ToString();
        //}


        //popupContentDiv.InnerHtml = closeButtonSpan.ToString() + scrollableContentDiv.ToString();
        //popupDiv.InnerHtml = popupContentDiv.ToString();

        //popupDiv.MergeAttribute("style", "display: flex;");
        //var script = new TagBuilder("script");
        //script.InnerHtml = "function togglePopup() { const popup = document.getElementById('popupOverlay'); popup.style.display = popup.style.display === 'none' ? 'flex' : 'none'; }";

        return popupOverlay.ToString();
    }

    public string GetPopupHtml()
    {
        return PopupHtml;
    }
}