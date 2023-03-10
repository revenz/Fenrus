using Fenrus.Services;
using Jint;
using Jint.Native;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Fenrus.Helpers.AppHelpers;
using Fenrus.Models;
using Humanizer;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for app 
/// </summary>
[Route("/apps")]
public class DashboardAppController: BaseController
{
    private readonly IMemoryCache Cache;
    
    /// <summary>
    /// Constructs an instance of Dashboard App Controller
    /// </summary>
    /// <param name="cache">memory cache</param>
    public DashboardAppController(IMemoryCache cache)
    {
        this.Cache = cache;
    }
    
    /// <summary>
    /// Gets an apps icon
    /// </summary>
    /// <param name="name">The app name</param>
    /// <returns>the app icon</returns>
    [HttpGet("{name}/{iconFile}")]
    [ResponseCache(Duration = 7 * 24 * 60 * 60)]
    public IActionResult Icon([FromRoute] string name, [FromRoute] string iconFile)
    {
        Logger.ILog("Getting app icon: " + name + ", file: " + iconFile);
        var app = AppService.GetByName(name);
        if (string.IsNullOrEmpty(app?.Icon))
        {
            Logger.ILog("Getting app icon: " + name + ", icon not found");
            return new NotFoundResult();
        }

        string appIcon = Path.Combine(app.FullPath, app.Icon);
        if (System.IO.File.Exists(appIcon) == false)
        {
            Logger.ILog("Getting app icon: " + name + ", icon not found: " + appIcon);
            return new NotFoundResult();
        }

        var image = System.IO.File.OpenRead(appIcon);
        string type = "image/" + appIcon.Substring(appIcon.LastIndexOf(".", StringComparison.Ordinal) + 1);
        if (type == "image/svg")
            type = "image/svg+xml";
        return File(image, type);
    }

    /// <summary>
    /// Gets a app instance and caches it
    /// </summary>
    /// <param name="name">the name of the app</param>
    /// <param name="uid">the users instance of the application</param>
    /// <returns>the app</returns>
    private AppInstance? GetAppInstance(string name, Guid uid)
    {
        string key = name + "_" + uid;
        if (Cache.TryGetValue<AppInstance>(key, out AppInstance value))
            return value;

        var userUid = GetUserUid();
        if(userUid == null)
            return null;
        
        var ai = AppHeler.GetAppInstance(name);
        if (ai == null)
            return null;

        var groups = new GroupService().GetAllForUser(userUid.Value) ?? new ();

        if (groups.SelectMany(x => x.Items).FirstOrDefault(x => x.Uid == uid) is AppItem userApp == false)
            return null;

        ai.UserApp = userApp;
        
        Cache.Set(key, ai, TimeSpan.FromMinutes(30));
        return ai;
    }

    /// <summary>
    /// Gets the status of a smart app
    /// </summary>
    /// <param name="name">The app name</param>
    /// <returns>the app status</returns>
    [HttpGet("{name}/{uid}/status")]
    [ResponseCache(NoStore = true)]
    public IActionResult Status([FromRoute] string name, [FromRoute] Guid uid, [FromQuery] string size)
    {
        var ai = GetAppInstance(name, uid);
        if (ai == null)
            return new NotFoundResult();
        var engine = ai.Engine;

        List<string> log = new();
        var utils = new Utils();

        var statusArgs = AppHeler.GetApplicationArgs(engine,
            ai.UserApp.ApiUrl?.EmptyAsNull() ?? ai.UserApp.Url,
            AppHeler.DecryptProperties(ai.UserApp.Properties),
            log: log,
            size: size,
            response: Response);
                
        engine.SetValue("statusArgs", statusArgs);
        engine.SetValue("statusArgsUtils", utils);
        engine.Execute(@"
statusArgs.Utils = statusArgsUtils;
var status = instance.status(statusArgs);");
        var result = engine.GetValue("status");
        result = result.UnwrapIfPromise();
        if (result == null)
            result = string.Empty;
        var str = result.ToString();
        if (log.Any())
        {
            string header = $" [{uid}] ";
            int pad = (100 - header.Length) / 2;
            if (pad < 3)
                pad = 3;
            header = new string('-', pad) + header + new string('-', pad);
            Logger.DLog($"\n" + header + "\nApplication: " + name + "\n" + string.Join("\n", log) + "\n" + new string('-', header.Length));
        }

        return Content(str ?? string.Empty);
    }
}

/// <summary>
/// An initialised instance of an Application
/// </summary>
class AppInstance
{
    /// <summary>
    /// Gets or sets the engine used to run this instance
    /// </summary>
    public Engine Engine { get; set; }
    
    /// <summary>
    /// Gets or sets the App this is an instance of
    /// </summary>
    public FenrusApp App { get; set; }
    
    /// <summary>
    /// Gets or sets the user app this is an instance for
    /// </summary>
    public AppItem UserApp { get; set; }
}