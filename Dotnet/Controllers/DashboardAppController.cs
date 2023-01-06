using System.Text.RegularExpressions;
using Fenrus.Services;
using Jint;
using Jint.Native;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for app 
/// </summary>
[Route("/apps")]
public class DashboardAppController: Controller
{
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
        
        var app = AppService.GetByName(name);
        if (app?.IsSmart != true)
            return new NotFoundResult();

        string codeFile = Path.Combine(app.FullPath, "code.js");
        if (System.IO.File.Exists(codeFile) == false)
            return new NotFoundResult();

        
        string code = System.IO.File.ReadAllText(codeFile);
        code += $"\n\nlet instance = new {app.Name}()\nexport {{ instance }};";
        

        var engine = new Engine(options =>
        {
        });
        engine.AddModule(app.Name, code);
        var module = engine.ImportModule(app.Name);
        
        var statusArgs = JsObject.FromObject(engine, new
        {
            url = "https://github.com/revenz/Fenrus/", 
            properties = new Dictionary<string, object>(),
            fetch = new Func<object, Task<string>>(Fetch)
        });
        engine.SetValue("statusArgs", statusArgs);

        var instance = module.Get("instance").AsObject();
        engine.SetValue("instance", instance);
        engine.Execute("var status = instance.status(statusArgs);");
        var result = engine.GetValue("status");
        //var result = engine.Invoke("status", thisObj: instance, arguments: new[] { statusArgs });
        //var result = instance.Get("status").AsFunctionInstance().Invoke(engine, statusArgs);
        result = result.UnwrapIfPromise();
        var str = result.ToString();
        return Content(str);
    }

    private async Task<string> Fetch(object parameters)
    {
        using HttpClient client = new HttpClient();
        string url = parameters as string;
        return await client.GetStringAsync(url);
    }
}