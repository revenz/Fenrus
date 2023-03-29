using System.Text;
using Blazored.Toast;
using JavaScriptEngineSwitcher.Core.Resources;

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
        long b;
        if (bytes is double d)
            b = (long)d;
        else if (long.TryParse(bytes?.ToString() ?? string.Empty, out b) == false)
            return "0 B";
        var order = 0;
        var sizes = new[] { "B", "KB", "MB", "GB", "TB" };
        while (b >= 1000 && order < sizes.Length - 1)
        {
            ++order;
            b /= 1000;
        }

        return b.ToString("#.##") + ' ' + sizes[order];
    }


    /// <summary>
    /// Formats time
    /// </summary>
    /// <param name="date">the date to format</param>
    /// <param name="showSeconds">if seconds should be shown</param>
    /// <returns>the formatted time</returns>
    public string formatTime(DateTime date, bool showSeconds = false)
    {
        var hour = date.Hour;
        var meridian = "am";
        if (hour >= 12)
        {
            meridian = "pm";
            hour -= hour == 12 ? 0 : 12;
        }

        if (hour == 0)
            hour = 12;

        if (showSeconds)
            return hour + ':' + date.Minute.ToString("00") + ':' + date.Second.ToString("00") + ' ' + meridian;

        return hour + ':' + date.Minute.ToString("00") + ' ' + meridian;
    }

    /// <summary>
    /// Formats millisecond time to words
    /// </summary>
    /// <param name="milliseconds">the number of milliseconds</param>
    /// <param name="showSeconds">if seconds should be shown</param>
    /// <returns>the milliseconds as words</returns>
    public string formatMilliTimeToWords(decimal milliseconds, bool showSeconds = false)
    {
        // should swap this out for Humanizer, copy from fenrus Node
        
        var days = Math.Floor(milliseconds / 1000 / 60 / 60 / 24);
        milliseconds -= days * 1000 * 60 * 60 * 24;
        var hour = Math.Floor(milliseconds / 1000 / 60 / 60);
        milliseconds -= hour * 1000 * 60 * 60;
        var minute = Math.Floor(milliseconds / 1000 / 60);
        milliseconds -= minute * 1000 * 60;
        var seconds = Math.Floor(milliseconds / 1000);

        var returnText = "";

        if (hour == 1)
            returnText = returnText + hour + " hour ";
        else if (hour > 1)
            returnText = returnText + hour + " hours ";


        if (minute == 1)
            returnText = returnText + minute + " minute ";
        else if (minute > 1)
            returnText = returnText + minute + " minutes ";

        if (showSeconds)
        {
            if (seconds == 1)
                return returnText + seconds + " second ";
            if (seconds > 1)
                return returnText + seconds + " seconds ";
        }

        return returnText;
    }

    /// <summary>
    /// Formats a date
    /// </summary>
    /// <param name="date">the date object to format</param>
    /// <returns>a formatted date</returns>
    public string formatDate(object date)
    {
        if (date == null)
            return string.Empty;

        DateTime dt;
        if (date is string str)
            dt = DateTime.Parse(str);
        else if (date is long l)
            dt = new DateTime(l);
        else if (date is DateTime datetime)
            dt = datetime;
        else
            return "Invalid date time object: " + date.GetType().FullName;

        var now = DateTime.Now;
        if (dt.Subtract(now).TotalDays < 1)
        {
            // within last 24 hours
            if (dt.Day == now.Day) {
                // today, so return time
                return this.formatTime(dt);
            }
        }

        return dt.ToString("yyyy-MM-dd");
    }
}