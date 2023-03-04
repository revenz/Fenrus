using Fenrus.Models;
using Fenrus.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for Up Time
/// </summary>
[Route("settings/up-time")]
public class UpTimeController : BaseController
{
    /// <summary>
    /// Gets the uptime data
    /// </summary>
    /// <param name="url">the URL to lookup the uptime for</param>
    [HttpGet]
    public IEnumerable<object> Get([FromQuery] string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return new object[] { };
        var settings = GetUserSettings();
        if (settings == null)
            throw new UnauthorizedAccessException();

        var service = new UpTimeService();
        var data = service.GetData(url);
        // we dont need to return the URL, so we return an anonymous object to avoid it
        return data.Select(x => new
        {
            x.Date,
            Up = x.Status,
            x.Message
        });
    }
}