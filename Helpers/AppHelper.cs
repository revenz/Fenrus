using Fenrus.Controllers;
using Fenrus.Helpers.AppHelpers;
using Fenrus.Models;
using Humanizer;
using Jint;
using Jint.Native;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Helpers;

/// <summary>
/// Helper to run apps
/// </summary>
public class AppHeler
{

    /// <summary>
    /// Gets an app instance
    /// </summary>
    /// <param name="name">the name of the application</param>
    /// <returns>the app instance</returns>
    internal static AppInstance? GetAppInstance(string name)
    {
        var app = AppService.GetByName(name);
        if (app?.IsSmart != true)
            return null;

        string codeFile = Path.Combine(app.FullPath, "code.js");
        if (File.Exists(codeFile) == false)
            return null;

        string appName = app.Name.Dehumanize();
        string code = File.ReadAllText(codeFile);
        code += $"\n\nlet instance = new {appName}()\nexport {{ instance }};";

        var engine = new Engine(options => { });
        engine.AddModule(appName, code);
        var module = engine.ImportModule(appName);

        var instance = module.Get("instance").AsObject();

        engine.SetValue("instance", instance);

        return new AppInstance
        {
            App = app,
            Engine = engine
        };
    }

    /// <summary>
    /// Gest the statusArgs which are passed to an app to run/test the application
    /// </summary>
    /// <param name="engine">The Jint engine</param>
    /// <param name="url">the application URL</param>
    /// <param name="linkTarget">the link target</param>
    /// <param name="size">the application size</param>
    /// <param name="log">the log</param>
    /// <param name="properties">the application properties</param>
    /// <param name="response">the requesting http response</param>
    /// <returns>the args to pass into an application</returns>
    public static JsValue GetApplicationArgs(Engine engine, string url,
        Dictionary<string, object> properties, List<string> log,
        string linkTarget,
        string? size = null,
        HttpResponse? response = null)
    {
        var utils = new Utils();

        var statusArgs = JsObject.FromObject(engine, new
        {
            url,
            size,
            linkTarget = linkTarget,
            properties = properties ?? new(),
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
            fetch = new Func<object, object>(parameters =>
                Fetch.Execute(new()
                {
                    Engine = engine,
                    AppUrl = url,
                    Parameters = parameters,
                    Log = log.Add
                }).Result
            ),
            log = new Action<string>(log.Add),
            carousel = Carousel.Instance,
            barInfo = new Func<BarInfo.BarInfoItem[], string>(items =>
                BarInfo.Generate(utils, items)
            ),
            liveStats = new Func<string[][], string>(items =>
                LiveStats.Generate(utils, items)
            ),
            changeIcon = new Action<string>(icon => { response?.Headers?.TryAdd("x-icon", utils.base64Encode(icon)); }),
            imageSearch = new Func<string, string[]>(ImageSearch.Search),
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
                    response?.Headers?.TryAdd("x-status-indicator",
                        indicator == "" ? indicator : utils.base64Encode(indicator));
                }
                catch (Exception)
                {
                    // can fail if request is aborted
                }
            })
        });
        return statusArgs;
    }

    /// <summary>
    /// Decrypts the properties and returns a NEW dictionary
    /// </summary>
    /// <param name="properties">the properties</param>
    /// <returns>a NEW dictionary</returns>
    public static Dictionary<string, object> DecryptProperties(Dictionary<string, object> properties)
        => properties?.ToDictionary(x => x.Key, x =>
        {
            if (x.Value is string str == false)
                return x.Value;
            if (EncryptionHelper.TryDecrypt(str, out string decrypted))
                return decrypted;
            return x.Value;
        }) ?? new();
}