using Fenrus.Services;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;

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

    private Engine GetAppInstance(string name, Guid uid)
    {
        string key = name + "_" + uid;
        if (Cache.TryGetValue<Engine>(key, out Engine value))
            return value;
        
        var app = AppService.GetByName(name);
        if (app?.IsSmart != true)
            return null;

        string codeFile = Path.Combine(app.FullPath, "code.js");
        if (System.IO.File.Exists(codeFile) == false)
            return null;
        
        string code = System.IO.File.ReadAllText(codeFile);
        code += $"\n\nlet instance = new {app.Name}()\nexport {{ instance }};";
        
        var engine = new Engine(options =>
        {
        });
        engine.AddModule(app.Name, code);
        var module = engine.ImportModule(app.Name);
        
        var instance = module.Get("instance").AsObject();
        engine.SetValue("instance", instance);

        Cache.Set(key, engine, TimeSpan.FromMinutes(30));
        return engine;
    }

    /// <summary>
    /// Gets the status of a smart app
    /// </summary>
    /// <param name="name">The app name</param>
    /// <returns>the app status</returns>
    [HttpGet("{name}/{uid}/status")]
    public IActionResult Status([FromRoute] string name, [FromRoute] Guid uid)
    {
        if (name != "GOG")
            return new NotFoundResult(); // for now

        var engine = GetAppInstance(name, uid);
        if (engine == null)
            return new NotFoundResult();
        
        var statusArgs = JsObject.FromObject(engine, new
        {
            url = "https://github.com/revenz/Fenrus/", 
            properties = new Dictionary<string, object>(),
            fetch = new Func<object, object>((parameters) =>
                Helpers.AppHelpers.Fetch.Instance(engine, parameters)
            ),
            log = new Action<object>(Console.WriteLine),
            carousel = Helpers.AppHelpers.Carousel.Instance,
            Utils = Helpers.AppHelpers.Utils.Instance
        });
        engine.SetValue("statusArgs", statusArgs);
        engine.Execute("var status = instance.status(statusArgs);");
        var result = engine.GetValue("status");
        result = result.UnwrapIfPromise();
        var str = result.ToString();
        return Content(str);
    }
}

class AppInstance
{
    public Engine Engine { get; set; }
    
    public ObjectInstance Instance { get; set; }

}