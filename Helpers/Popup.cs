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
        closeButtonSpan.MergeAttribute("onclick", $"closePopup('{popupid}')");

        var popupTitle = new TagBuilder("h2");
        popupTitle.GenerateId("popupTitle");
        popupTitle.MergeAttribute("style", "text-align: center;");
        popupTitle.InnerHtml = title;

        popupTop.InnerHtml = backButtonSpan.ToString() + popupTitle.ToString() + closeButtonSpan.ToString();
        
        popupDiv.InnerHtml += popupTop.ToString();

        foreach (var element in elements)
        {
            popupDiv.InnerHtml += element.ToString();
        }

        popupOverlay.InnerHtml = popupDiv.ToString();

        return popupOverlay.ToString();
    }

    public string GetPopupHtml()
    {
        return PopupHtml;
    }
}