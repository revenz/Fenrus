namespace Fenrus.Helpers.AppHelpers;

/// <summary>
/// Utils for apps
/// </summary>
public class Utils
{
    /// <summary>
    /// Gets the instance of the utils
    /// </summary>
    public static readonly object Instance = new
    {
        newGuid = new Func<string>(() => Guid.NewGuid().ToString()),
        htmlEncode = new Func<string, string>(text =>
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                .Replace("\"", "&#34;").Replace("'", "&#39;");
        }),
        base64Encode = new Func<string, string>(str =>
        {
            if (String.IsNullOrEmpty(str)) return string.Empty;
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(plainTextBytes);
        }),
        base64Decode = new Func<string, string>(str =>
        {
            if (String.IsNullOrEmpty(str)) return string.Empty;
            var base64EncodedBytes = System.Convert.FromBase64String(str);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }),
        formatBytes = new Func<object, object>(bytes =>
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
        })
    };
}