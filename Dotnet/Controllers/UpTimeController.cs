using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

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
    /// <param name="uid">the app instance to get the up time for</param>
    [HttpGet("{uid}")]
    public IActionResult Get([FromRoute] Guid uid)
    {
        var jsonContent = MediaTypeHeaderValue.Parse("application/json");
        if (uid == Guid.Empty)
            return Content("[]", jsonContent);
        var settings = GetUserSettings();
        if (settings == null)
            throw new UnauthorizedAccessException();

        var app = settings.Groups.SelectMany(x => x.Items).FirstOrDefault(x => x.Uid == uid);
        if (app == null)
            return Content("[]", jsonContent);
        
        var file = Path.Combine(DirectoryHelper.GetUpTimeDirectory(), settings.Uid.ToString(), uid + ".json");
        if(System.IO.File.Exists(file) == false)
            return Content("[]", jsonContent);
        string json = System.IO.File.ReadAllText(file);
        return Content(json, MediaTypeHeaderValue.Parse("application/json"));
    }
}