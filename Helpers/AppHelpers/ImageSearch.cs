using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace Fenrus.Helpers.AppHelpers;

/// <summary>
/// Image search helper
/// </summary>
public class ImageSearch
{
    /// <summary>
    /// Searches for an image and returns any URLs found
    /// </summary>
    /// <param name="query">the query to search</param>
    /// <returns>a list URLs to images found</returns>
    public static string[] Search(string query)
    {
        string html = GetHtmlCode(query);
        if (string.IsNullOrEmpty(html)) return new string[] { };
        var urls = GetUrls(html);
        return urls?.ToArray() ?? new string [] { };
    }
    
    
    private static string GetHtmlCode(string query)
    {
        var rnd = new Random();
        string url = "https://www.google.com/search?q=" + HttpUtility.UrlPathEncode(query) + "&tbm=isch";
        string data = "";

        var request = (HttpWebRequest)WebRequest.Create(url);
        request.Accept = "text/html, application/xhtml+xml, */*";
        request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";

        var response = (HttpWebResponse)request.GetResponse();

        using Stream dataStream = response.GetResponseStream();
        using var sr = new StreamReader(dataStream);
        data = sr.ReadToEnd();

        return data;
    }
    
    private static List<string> GetUrls(string html)
    {
        html = html.Substring(html.LastIndexOf("chips=") + 5);
        html = html.Substring(html.IndexOf("<table class"));
        
        var urls = new List<string>();

        var matches = Regex.Matches(html, "<td(.*?)<img(.*?)</td>");
        foreach(Match match in matches)
        {
            string inner = match.Value;
            var src = Regex.Match(inner, "src=\"([^\"]+)\"");
            if (src.Success == false)
                continue;
            string url = src.Groups[1].Value;
            url = HttpUtility.HtmlDecode(url);
            urls.Add(url);
        }

        // string search = @",""ou"":""(.*?)"",";
        // var matches = Regex.Matches(html, search);
        //
        // foreach (Match match in matches)
        // {
        //     urls.Add(match.Groups[1].Value);
        // }

        return urls;
    }
}