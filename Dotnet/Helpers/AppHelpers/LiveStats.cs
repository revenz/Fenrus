using System.Text.RegularExpressions;

namespace Fenrus.Helpers.AppHelpers;

/// <summary>
/// Live Stats helper
/// </summary>
public class LiveStats
{

    /// <summary>
    /// Generates the live stats reponse
    /// </summary>
    public static string Generate(Utils utils, string[][] items)
    {
        var html = "<ul class=\"livestats\">";
        var rgxHtml = new Regex("^:html:");
        foreach (var item in items)
        {
            for (int i = 0; i < item.Length; i++)
            {
                if (rgxHtml.IsMatch(item[i]) == false)
                    item[i] = utils.htmlEncode(item[i]);
                else
                    item[i] = item[i][6..];
            }

            if (item.Length == 1)
            {
                // special case, this is doing a span
                html += $"<li><span class=\"title span\">{item[0]}</span></li>";
            }
            else
            {
                html += $"<li><span class=\"title\">{item[0]}</span><span class=\"value\">{item[1]}</span></li>";
            }
        }

        html += "</ul>";
        return html;
    }
}