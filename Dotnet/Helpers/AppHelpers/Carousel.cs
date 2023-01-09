namespace Fenrus.Helpers.AppHelpers;

/// <summary>
/// Carousel helper
/// </summary>
public class Carousel
{
    /// <summary>
    /// Gets the instance of the carousel
    /// </summary>
    public static readonly object Instance = new Func<string[], string>(items =>
    {
        var id = Guid.NewGuid().ToString("N");
        var html = $":carousel:{id}:<div class=\"carousel\" id=\"{id}\">";
        var controls = "<div class=\"controls\" onclick=\"event.stopImmediatePropagation();return false;\">";
        var count = 0;
        foreach (var item in items)
        {
            var itemId = Guid.NewGuid();
            html += $"<div class=\"item {(count == 0 ? "visible initial" : "")}\" id=\"{id}-{count}\">";
            html += item;
            html += "</div>";
            controls += $"<a href=\"#{itemId}\" class=\"{(count == 0 ? "selected" : "")}\"></a>";
            ++count;
        }

        controls += "</div>";
        html += controls;
        html += "</div>";
        return html;
    });
}