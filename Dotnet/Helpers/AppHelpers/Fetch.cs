using Jint;

namespace Fenrus.Helpers.AppHelpers;

/// <summary>
/// Fetch helper
/// </summary>
public class Fetch
{
    /// <summary>
    /// Gets an instance of the fetch helper
    /// </summary>
    public static readonly Func<Engine, object, object> Instance = (engine, parameters) =>
    {
        using HttpClient client = new HttpClient();
        var request = new HttpRequestMessage();
        string url;
        if (parameters is string str)
        {
            url = str;
            request.Headers.Add("Accept", "application/json");
        }
        else
        {
            url = "test";
            // if (!args.headers)
            //     args.headers = { 'Accept': 'application/json' };
            // else if (!args.headers['Accept'])
            //     args.headers['Accept'] = 'application/json';
        }
        request.RequestUri = new Uri(url);
        request.Method = HttpMethod.Get;
        var result = client.SendAsync(request).Result;
        string content = result.Content.ReadAsStringAsync().Result;
        //return content;
        
        var trimmed = content.Trim();
        if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
        {
            var parsed = engine.Evaluate($"JSON.parse(`{content}`)").ToObject();
            return parsed;
        }

        if (trimmed == "true") return true;
        if (trimmed == "false") return false;
        if (double.TryParse(trimmed, out double dbl))
            return dbl;
        return content;
    };
}