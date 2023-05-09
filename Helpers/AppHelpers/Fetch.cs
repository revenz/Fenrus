using System.Diagnostics;
using System.Net;
using Jint;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace Fenrus.Helpers.AppHelpers;

/// <summary>
/// Fetch helper
/// </summary>
public class Fetch
{
    public class FetchArgs
    {
        public Engine Engine { get; set; }
        public string AppUrl { get; set; }
        public object Parameters { get; set; }
        public Action<string> Log { get; set; }
    }
    
    /// <summary>
    /// Gets an instance of the fetch helper
    /// </summary>
    public static async Task<FetchResult> Execute(FetchArgs args)
    {
        try
        {
            var engine = args.Engine;
            var appUrl = args.AppUrl;
            var parameters = args.Parameters;

            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            using HttpClient client = new HttpClient(handler);
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            string url;
            int timeout = 10;
            if (parameters is string str)
            {
                url = str;
                request.Headers.Add("Accept", "application/json");
            }
            else
            {
                var fp = JsonSerializer.Deserialize<FetchParameters>(
                    JsonSerializer.Serialize(parameters)
                    , new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    });
                url = fp.Url;
                if (fp.Timeout > 0)
                {
                    timeout = fp.Timeout;
                }

                if (string.IsNullOrEmpty(fp.Body) == false)
                {
                    request.Content = new StringContent(fp.Body);
                }
                request.Headers.Clear();
                fp.Headers ??= new();
                if (fp.Headers.ContainsKey("Accept") == false)
                    fp.Headers.Add("Accept", "application/json");
                foreach (var header in fp.Headers)
                {
                    try
                    {
                        if (header.Key.Replace("-", "").ToLowerInvariant() == "ContentType".ToLowerInvariant())
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(header.Value);
                        else
                            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                    catch (Exception)
                    {
                    }
                }
                

                request.Method = fp.Method?.ToLower() switch
                {
                    "post" => HttpMethod.Post,
                    "put" => HttpMethod.Put,
                    "delete" => HttpMethod.Delete,
                    "patch" => HttpMethod.Patch,
                    _ => HttpMethod.Get
                };
            }

            if (url.StartsWith("http") == false)
            {
                if (appUrl.EndsWith('/') == false)
                    url = appUrl + '/' + url;
                else
                    url = appUrl + url;
            }

            request.RequestUri = new Uri(url);

            var (success, result) = Send(client, request, timeout);
            if (success == false)
                throw new Exception("Timeout");
            
            var trimmed = result.content.Trim();
            if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
            {
                try
                {
                    engine.SetValue("temp_json", result.content);
                    var parsed = engine.Evaluate("JSON.parse(temp_json)").ToObject();
                    result.data = parsed;
                }
                catch (Exception ex)
                {
                }
            }
            else if (trimmed == "true") result.data = true;
            else if (trimmed == "false") result.data = false;
            else if (double.TryParse(trimmed, out double dbl))
                result.data = dbl;
            return result;
        }
        catch (Exception ex)
        {
            return new ()
            {
                content = ex.Message,
                cookies = new (),
                headers = new (),
                status = 500,
                success = false
            };
        }
    }

    /// <summary>
    /// Sends a request and returns the result
    /// </summary>
    /// <param name="client">the HTTP Client</param>
    /// <param name="request">the message to send</param>
    /// <param name="timeout">the timeout in seconds for the request</param>
    /// <returns>the result of the request</returns>
    private static (bool success, FetchResult result) Send(HttpClient client, HttpRequestMessage request, int timeout)
    {
        bool success = false;
        bool done = false;
        FetchResult sendResult = new();

        var send = Task.Run(() =>
        {
            client.Timeout = TimeSpan.FromSeconds(timeout);
            var cts = new CancellationTokenSource();

            var response = client.SendAsync(request, cts.Token).Result;
            if (done)
                return;
            
            sendResult.content = response.Content.ReadAsStringAsync(cts.Token).Result;
            sendResult.data = sendResult.content;
            success = true;
            sendResult.success = response.IsSuccessStatusCode;
            sendResult.status = (int)response.StatusCode;
            sendResult.cookies = new();
            var cookies = new List<Cookie>();
            if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
            {
                foreach (var setCookieHeader in setCookieHeaders)
                {
                    var setCookie = SetCookieHeaderValue.Parse(setCookieHeader);
                    var cookie = new Cookie(setCookie.Name.ToString(), setCookie.Value.ToString(), setCookie.Path.ToString(), setCookie.Domain.ToString());

                    sendResult.cookies[cookie.Name] = cookie.Value;
                }
            }
            sendResult.headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.First());
            
        });

        Task.WhenAny(send, Task.Delay(timeout * 1_000)).Wait();
        done = true;
        
        return (success, sendResult);
    }

    /// <summary>
    /// Fetch parameters
    /// </summary>
    class FetchParameters
    {
        /// <summary>
        /// Gets or sets the URL to get
        /// </summary>
        public string Url { get; set;}
        /// <summary>
        /// Gets or sets timeout in seconds
        /// </summary>
        public int Timeout { get; set; }
        
        /// <summary>
        /// Gets or sets the request Method
        /// </summary>
        public string Method { get; set; }
        
        /// <summary>
        /// Gets or sets request headers
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Gets or sets the body of the request
        /// </summary>
        public string Body { get; set; }
    }

    /// <summary>
    /// A result from a fetch
    /// Note: these properties are in camelCase to better match Javascript
    /// </summary>
    public class FetchResult
    {
        /// <summary>
        /// Gets the status code from the fetch
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// Gets if the request was successful
        /// </summary>
        public bool success { get; set; }
        /// <summary>
        /// Gets any response headers
        /// </summary>
        public Dictionary<string, string> headers { get; set; }
        /// <summary>
        /// Gets the response body
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// Gets or sets the parsed response of the content
        /// </summary>
        public object data { get; set; }
        /// <summary>
        /// Gets any response cookies returned
        /// </summary>
        public Dictionary<string, string> cookies { get; set; }
    }
}