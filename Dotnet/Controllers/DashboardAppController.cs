using Fenrus.Services;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using Fenrus.Helpers.AppHelpers;
using Fenrus.Models;
using Fenrus.Pages;
using Humanizer;
using Microsoft.AspNetCore.Authorization;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for app 
/// </summary>
[Route("/apps")]
public class DashboardAppController: Controller
{
    private readonly IMemoryCache Cache;
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
        var app = AppService.GetByName(name);
        if (string.IsNullOrEmpty(app?.Icon))
            return new NotFoundResult();

        string appIcon = Path.Combine(app.FullPath, app.Icon);
        if(System.IO.File.Exists(appIcon) == false)
            return new NotFoundResult();

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
        
        // need to move this into a helper method, or a base class
        var sid = User?.Claims?.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid")?.Value;
        if (string.IsNullOrEmpty(sid) || Guid.TryParse(sid, out Guid userUid) == false)
            return null;

        var settings = new Services.UserSettingsService().Load(userUid);
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
        var utils = new Helpers.AppHelpers.Utils();
        var statusArgs = JsObject.FromObject(engine, new
        {
            url = "https://github.com/revenz/Fenrus/", 
            size,
            properties = new Dictionary<string, object>(),
            fetch = new Func<object, Task<object>>(async (parameters) =>
                await Fetch.Execute(new ()
                {
                    Engine = engine,
                    AppUrl = ai.UserApp.ApiUrl?.EmptyAsNull() ?? ai.UserApp.Url,
                    Parameters = parameters,
                    Log = text =>
                    {
                        log.Add(text);
                    }
                })
            ),
            log = new Action<string>(text =>
            {
                log.Add(text);
            }),
            carousel = Carousel.Instance,
            barInfo = new Func<BarInfo.BarInfoItem[], string>(items =>
                BarInfo.Generate(utils, items)
            ),
            changeIcon = new Action<string>(icon =>
            {
                Response.Headers.TryAdd("x-icon", utils.base64Encode(icon));
            }),
            setStatusIndicator = new Action<string>(indicator =>
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
            })
        });
        engine.SetValue("statusArgs", statusArgs);
        engine.SetValue("statusArgsUtils", utils);
        engine.Execute(@"
statusArgs.Utils = statusArgsUtils;
var status = instance.status(statusArgs);");
        var result = engine.GetValue("status");
        result = result.UnwrapIfPromise();
        var str = result.ToString();
        return Content(str);
    }
}

class AppInstance
{
    public Engine Engine { get; set; }
    
    public FenrusApp App { get; set; }
    
    public AppItem UserApp { get; set; }

}