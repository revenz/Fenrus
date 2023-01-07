using System.Text;
using Blazored.Toast;

namespace Fenrus.Helpers.AppHelpers;

/// <summary>
/// Utils for apps
/// </summary>
public class Utils
{
    /// <summary>
    /// Creates a new GUID
    /// </summary>
    /// <returns>a new GUID</returns>
    public object newGuid() => Guid.NewGuid().ToString();

    public string btoa(object o)
    {
        if (o == null) return string.Empty;
        byte[] bytes = Encoding.GetEncoding(28591).GetBytes(o.ToString());
        return Convert.ToBase64String(bytes);
    }

    
    public string atoa(string text)
    {
        return Encoding.GetEncoding(28591).GetString(Convert.FromBase64String(text));
    }

    public string htmlEncode(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
            .Replace("\"", "&#34;").Replace("'", "&#39;");
    }


    /// <summary>
    /// Base 64 encodes a string
    /// </summary>
    public string base64Encode(string str)
    {
        if (String.IsNullOrEmpty(str)) return string.Empty;
        var plainTextBytes = Encoding.UTF8.GetBytes(str);
        return Convert.ToBase64String(plainTextBytes);
    }

    public string base64Decode(string str)
    {
        if (String.IsNullOrEmpty(str)) return string.Empty;
        var base64EncodedBytes = Convert.FromBase64String(str);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }

    public string formatBytes(object bytes)
    {
        if (long.TryParse(bytes?.ToString() ?? string.Empty, out long b))
            return string.Empty;
        var order = 0;
        var sizes = new[] { "B", "KB", "MB", "GB", "TB" };
        while (b >= 1000 && order < sizes.Length - 1)
        {
            ++order;
            b /= 1000;
        }

        return b.ToString("#.##") + ' ' + sizes[order];
    }
}