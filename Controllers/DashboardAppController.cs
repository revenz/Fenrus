using Jint;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Fenrus.Helpers.AppHelpers;
using Fenrus.Models;

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
    /// <param name="userUid">the UID of hte user</param>
    /// <param name="name">the name of the app</param>
    /// <param name="uid">the users instance of the application</param>
    /// <returns>the app</returns>
    private AppInstance? GetAppInstance(Guid userUid, string name, Guid uid)
    {

        List<Group> groups;
        string groupKey = userUid + "_Groups";
        lock (Cache)
        {
            if (Cache.TryGetValue<List<Group>>(groupKey, out groups) == false)
            {
                var service = new GroupService();
                groups = service.GetAllForUser(userUid).Union(service.GetSystemGroups(enabledOnly: true)).ToList();
                // short cache of ths, just so if many apps are requesting at once
                Cache.Set(groupKey, groups, TimeSpan.FromSeconds(30));
            }
        }


        if (groups.SelectMany(x => x.Items).FirstOrDefault(x => x.Uid == uid) is AppItem userApp == false)
            return null;
        
        string key = name + "_" + uid;
        if (Cache.TryGetValue<AppInstance>(key, out AppInstance value))
        {
            // need to check updated settings
            value.UserApp = userApp;
            return value;
        }

        
        var ai = AppHeler.GetAppInstance(name);
        if (ai == null)
            return null;

        ai.UserApp = userApp;
        
        Cache.Set(key, ai, TimeSpan.FromMinutes(30));
        return ai;
    }

    /// <summary>
    /// Gets the status of a smart app
    /// </summary>
    /// <param name="name">The app name</param>
    /// <param name="dashboardUid">The UID of the dashboard this app is updating on</param>
    /// <param name="uid">the UID of the smart app instance</param>
    /// <param name="size">the size of the app being updated</param>
    /// <returns>the app status</returns>
    [HttpGet("dashboard/{dashboardUid}/{name}/{uid}/status")]
    [ResponseCache(NoStore = true)]
    public IActionResult Status([FromRoute] Guid dashboardUid, [FromRoute] string name, [FromRoute] Guid uid, [FromQuery] string size)
    {
        List<string> log = new();
        try
        {
            var userUid = GetUserUid();
            if(userUid == null)
                return null;
            
            var ai = GetAppInstance(userUid.Value, name, uid);
            if (ai == null)
                return new NotFoundResult();
            var engine = ai.Engine;
            var utils = new Utils();
            var dashboard = new DashboardService().GetByUid(dashboardUid);
            if (dashboard.Uid != Globals.GuestDashbardUid && dashboard.UserUid != userUid.Value)
                return null;

            var theme = new ThemeService().GetTheme(dashboard.Theme);
            if (theme?.ForcedSize != null)
                size = theme.ForcedSize.Value.ToString().ToLower();
            
            string linkTarget = dashboard.LinkTarget;

            var statusArgs = AppHeler.GetApplicationArgs(engine,
                ai.UserApp.ApiUrl?.EmptyAsNull() ?? ai.UserApp.Url,
                AppHeler.DecryptProperties(ai.UserApp.Properties),
                linkTarget: linkTarget,
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
            // if (log.Any() && ai.UserApp.Debug)
            // {
            //     string header = $" [{uid}] ";
            //     int pad = (100 - header.Length) / 2;
            //     if (pad < 3)
            //         pad = 3;
            //     header = new string('-', pad) + header + new string('-', pad);
            //     Logger.DLog($"\n" + header + "\nApplication: " + name + "\n" + string.Join("\n", log) + "\n" +
            //                 new string('-', header.Length));
            // }
            
            SmartAppCache.AddData(uid, true, log?.Any() == true ? string.Join("\n", log) : string.Empty, str ?? string.Empty);

            return Content(str ?? string.Empty);
        }
        catch (Exception ex)
        {
            string exception = ex.Message;
            if (exception.StartsWith("Promise was rejected with value "))
                exception = exception.Substring("Promise was rejected with value ".Length);
            string msg = exception + (log.Any() ? "\n" + string.Join("\n", log) : string.Empty);
            Logger.WLog("Error in DashboardApp.Status: " + name + " -> " + msg);


            SmartAppCache.AddData(uid, false, msg, string.Empty);

            return Content("ERR:" + msg);
        }
    }

    /// <summary>
    /// Gets a list of smart update requests
    /// </summary>
    /// <param name="uid">The UID of the smart app</param>
    /// <returns>a list of historic update requests</returns>
    [HttpGet("{uid}/history/list")]
    public IEnumerable<object> GetAppUpdateHistoryList([FromRoute] Guid uid)
        => SmartAppCache.GetData(uid).OrderByDescending(x => x.Date).Select(x => new 
        {
            date = x.Date,
            success = x.Success
        });

    /// <summary>
    /// Gets a single of smart update requests
    /// </summary>
    /// <param name="uid">The UID of the smart app</param>
    /// <param name="dateUtc">The date of the update</param>
    /// <returns>a single update historic record, if exists</returns>
    [HttpGet("{uid}/history/{date}")]
    public SmartAppCacheItemData? GetAppUpdateHistoryItem([FromRoute] Guid uid, [FromRoute] DateTime date)
    {
        var list =  SmartAppCache.GetData(uid);
        
        var item = list.FirstOrDefault(x => x.Date == date);
        return item;
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