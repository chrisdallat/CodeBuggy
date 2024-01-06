using System.Web.Mvc;

namespace CodeBuggy.Helpers;
public class Popup
{
    public class Field
    {
        public string? Label { get; set; }
        public string? Value { get; set; }
        public string? Type { get; set; }
    }

    public string? Title { get; set; }
    public Field[]? Fields { get; set; }

    public string? PopupHtml { get; set; }

    public void Create()
    {
        var popupDiv = new TagBuilder("div");
        popupDiv.MergeAttribute("id", "popupOverlay");
        popupDiv.MergeAttribute("class", "overlay");
        popupDiv.MergeAttribute("style", "display: flex;");

        var popupContentDiv = new TagBuilder("div");
        popupContentDiv.MergeAttribute("class", "popup");

        var closeButtonSpan = new TagBuilder("span");
        closeButtonSpan.MergeAttribute("class", "close");
        closeButtonSpan.MergeAttribute("onclick", "togglePopup()");
        closeButtonSpan.InnerHtml = "&times;";

        var scrollableContentDiv = new TagBuilder("div");
        foreach (var field in Fields) 
        {
            var container = new TagBuilder("div");
            container.AddCssClass("containerBox");

            var label = new TagBuilder("label");
            label.AddCssClass("left-div");
            label.InnerHtml = field.Label;
            
            switch (field.Type)
            {
                case "Input":
                    var input = new TagBuilder("Input");
                    input.AddCssClass("right-div");
                    container.InnerHtml = label.ToString() + input.ToString(); 
                    break;

                default:
                    break;
            }

            scrollableContentDiv.InnerHtml += container.ToString();
        }
        

        popupContentDiv.InnerHtml = closeButtonSpan.ToString() + scrollableContentDiv.ToString();
        popupDiv.InnerHtml = popupContentDiv.ToString();

        popupDiv.MergeAttribute("style", "display: flex;");
        var script = new TagBuilder("script");
        script.InnerHtml = "function togglePopup() { const popup = document.getElementById('popupOverlay'); popup.style.display = popup.style.display === 'none' ? 'flex' : 'none'; }";

        PopupHtml = popupDiv.ToString() + script.ToString();
    }

    public string GetPopupHtml()
    {
        return PopupHtml;
    }
}