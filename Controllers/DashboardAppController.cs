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

    private AppInstance GetAppInstance(string name, Guid uid)
    {
        string key = name + "_" + uid;
        if (Cache.TryGetValue<AppInstance>(key, out AppInstance value))
            return value;
        
        var app = AppService.GetByName(name);
        if (app?.IsSmart != true)
            return null;
        
        var settings = GetUserSettings();
        if(settings == null)
            return null;

        if (settings.Groups.SelectMany(x => x.Items).FirstOrDefault(x => x.Uid == uid) is AppItem userApp == false)
            return null;

        string codeFile = Path.Combine(app.FullPath, "code.js");
        if (System.IO.File.Exists(codeFile) == false)
            return null;
        
        string appName = app.Name.Dehumanize();
        string code = System.IO.File.ReadAllText(codeFile);
        code += $"\n\nlet instance = new {appName}()\nexport {{ instance }};";
        
        var engine = new Engine(options =>
        {
        });
        engine.AddModule(appName, code);
        var module = engine.ImportModule(appName);
        
        var instance = module.Get("instance").AsObject();

        engine.SetValue("instance", instance);

        var ai = new AppInstance
        {
            App = app,
            UserApp = userApp,
            Engine = engine
        };
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
        
        var statusArgs = JsObject.FromObject(engine, new
        {
            url = ai.UserApp.ApiUrl?.EmptyAsNull() ?? ai.UserApp.Url, 
            size,
            properties = ai.UserApp.Properties ?? new (),
            humanizer = new Helpers.AppHelpers.Humanizer(),   
            // doesnt work if await, returns a Task to jint for some reason
            // fetch = new Func<object, Task<object>>(async (parameters) =>
            //     await Fetch.Execute(new ()
            //     {
            //         Engine = engine,
            //         AppUrl = ai.UserApp.ApiUrl?.EmptyAsNull() ?? ai.UserApp.Url,
            //         Parameters = parameters,
            //         Log = text =>
            //         {
            //             log.Add(text);
            //         }
            //     })
            // ),
            proxy = new Func<string, string>(url =>
                "/proxy/" + utils.base64Encode(url).Replace("/", "-")
            ),
            chart = new Chart(),
            fetch = new Func<object, object>((parameters) =>
                Fetch.Execute(new ()
                {
                    Engine = engine,
                    AppUrl = ai.UserApp.ApiUrl?.EmptyAsNull() ?? ai.UserApp.Url,
                    Parameters = parameters,
                    Log = text =>
                    {
                        #if(DEBUG)
                        Console.WriteLine(name + ": " + text);
                        #endif
                        log.Add(text);
                    }
                }).Result
            ),
            log = new Action<string>(text =>
            {
                log.Add(text);
            }),
            carousel = Carousel.Instance,
            barInfo = new Func<BarInfo.BarInfoItem[], string>(items =>
                BarInfo.Generate(utils, items)
            ),
            liveStats = new Func<string[][], string>(items => 
                LiveStats.Generate(utils, items)
            ),
            changeIcon = new Action<string>(icon =>
            {
                Response.Headers.TryAdd("x-icon", utils.base64Encode(icon));
            }),
            imageSearch = new Func<string, string[]>(ImageSearch.Search ),
            setStatusIndicator = new Action<string>(indicator =>
            {
                try
                {
                    indicator = (indicator ?? string.Empty).ToLower();
                    if (indicator.StartsWith("pause"))
                        indicator = "/common/status-icons/paused.png";
                    else if (indicator.StartsWith("record"))
                        indicator = "/common/status-icons/recording.png";
                    else if (indicator.StartsWith("stop"))
                        indicator = "/common/status-icons/stop.png";
                    else if (indicator.StartsWith("update"))
                        indicator = "/common/status-icons/update.png";
                    Response.Headers.TryAdd("x-status-indicator",
                        indicator == "" ? indicator : utils.base64Encode(indicator));
                }
                catch (Exception)
                {
                    // can fail if request is aborted
                } 
            })
        });
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