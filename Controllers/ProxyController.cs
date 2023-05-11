using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.Formats.Webp;

namespace Fenrus.Controllers;

/// <summary>
/// Proxy Controller
/// </summary>
[Route("proxy")]
public class ProxyController: BaseController
{
    /// <summary>
    /// Proxies a resource
    /// </summary>
    /// <param name="url">the base64 encoded resource</param>
    /// <returns>the proxied resource</returns>
    [HttpGet("{url}")]
    public async Task Get([FromRoute] string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Response.StatusCode = 404;
            return;
        }
        
        // check if guest is allow and if not, check if user is logged in
        var settings = GetUserSettings();
        if (settings == null)
        {
            // need to check global settings since the user is not signed in
            var system = GetSystemSettings();
            if (system?.AllowGuest != true)
            {
                // unauthorized
                Response.StatusCode = 401;
                return;
            }
        }
        
        // decode the url
        url = url.Replace("-", "/");
        url = new Helpers.AppHelpers.Utils().base64Decode(url);
        if (string.IsNullOrWhiteSpace(url))
        {
            Response.StatusCode = 404;
            return;
        }

        try
        {
            var result = await Globals.Client.SendAsync(new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get
            }, new CancellationTokenSource(5000).Token);
            
            int code = (int)result.StatusCode;
            Response.StatusCode = code;
            if (code >= 200 && code <= 299)
            {
                Response.Headers.Add("Cache-Control", "public, max-age=" + (31 * 24 * 60 * 60));
            }
            if(result.Content.Headers.ContentType != null)
                Response.Headers.Add("Content-Type", result.Content.Headers.ContentType.MediaType);

            using var responseStream = await result.Content.ReadAsStreamAsync().ConfigureAwait(false);
            await responseStream.CopyToAsync(Response.Body, 1024).ConfigureAwait(false);// 81920)).ConfigureAwait(false);
        }
        catch (Exception)
        {
            Response.StatusCode = 500;
        }
    }

    /// <summary>
    /// A proxy to a safe image
    /// </summary>
    /// <param name="url">the url of the image</param>
    /// <returns>the image download</returns>
    [HttpGet("safe-image/{url}")]
    public async Task SafeImageProxy([FromRoute] string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Response.StatusCode = 404;
            return;
        }
        url = url.Replace("-", "/");
        url = new Helpers.AppHelpers.Utils().base64Decode(url);
        if (string.IsNullOrWhiteSpace(url))
        {
            Response.StatusCode = 404;
            return;
        }

        string mimeType = null;
        try
        {
            var result = await Globals.Client.SendAsync(new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get
            }, new CancellationTokenSource(5000).Token);
            
            int code = (int)result.StatusCode;
            Response.StatusCode = code;
            if (code >= 200 && code <= 299)
            {
                Response.Headers.Add("Cache-Control", "public, max-age=" + (31 * 24 * 60 * 60));
            }

            mimeType = result.Content.Headers.ContentType.MediaType;

            using var responseStream = await result.Content.ReadAsStreamAsync().ConfigureAwait(false);

            using var image = Image.Load(responseStream);
            // Resize the image if it's too large
            if (image.Width > 800 || image.Height > 800)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(800, 800),
                    Mode = ResizeMode.Max
                }));
            }

            Response.Headers.Add("Content-Type", "image/webp");
            await image.SaveAsync(Response.Body, new WebpEncoder()).ConfigureAwait(false);
            
            //await responseStream.CopyToAsync(Response.Body, 1024).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Response.StatusCode = 500;
        }
    }
}