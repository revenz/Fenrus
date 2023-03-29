using Fenrus.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Initial configuration controller
/// </summary>
[Route("init-config")]
public class InitConfigController : Controller
{
    /// <summary>
    /// Gets or sets the application lifetime instance used to restart the application
    /// </summary>
    private IHostApplicationLifetime ApplicationLifetime { get; set; }
    
    /// <summary>
    /// Constructs an instance of the init config controller
    /// </summary>
    /// <param name="appLifetime">the application lifetime used to restart the application</param>
    public InitConfigController(IHostApplicationLifetime appLifetime)
    {
        ApplicationLifetime = appLifetime;
    }
    
    /// <summary>
    /// Gets the initial configuration page
    /// </summary>
    /// <returns>the initial configuration page</returns>
    [HttpGet]
    public IActionResult Index()
    {
        if (SystemSettingsService.InitConfigDone)
            return Redirect("/");
        
        string url = HttpContext.Request.GetDisplayUrl();
        url = url[..(url.IndexOf('/', 8) + 1)];
        
        ViewBag.RedirectUrl = url + "signin-oidc";
        
        return View("Index", new InitConfigModel()
        {
            Strategy = AuthStrategy.LocalStrategy,
            LocalStrategyUsername = "admin",
            OAuthStrategyIssuerBaseUrl = "https://accounts.google.com/", 
            OAuthStrategyBaseUrl = url
        });
    }

    /// <summary>
    /// Saves the initial configuration
    /// </summary>
    /// <returns>the save result</returns>
    [HttpPost]
    public IActionResult Save([FromForm] InitConfigModel model)
    {
        if (SystemSettingsService.InitConfigDone)
            return Redirect("/");
        
        var service = new SystemSettingsService();
        var settings = service.Get();
        settings.Strategy = model.Strategy;
        Logger.ILog("Saving initial config with strategy: " + model.Strategy);
        if (model.Strategy == AuthStrategy.LocalStrategy)
        {
            if (string.IsNullOrWhiteSpace(model.LocalStrategyUsername))
            {
                model.Error = "Username is required";
                return View("Index", model);
            }
            if (string.IsNullOrWhiteSpace(model.LocalStrategyPassword))
            {
                model.Error = "Password is required";
                return View("Index", model);
            }
            settings.OAuthStrategySecret = string.Empty;
            settings.OAuthStrategyBaseUrl = string.Empty;
            settings.OAuthStrategyClientId = string.Empty;
            settings.OAuthStrategyIssuerBaseUrl = string.Empty;
        }
        else
        {
            
            if (string.IsNullOrWhiteSpace(model.OAuthStrategyIssuerBaseUrl))
            {
                model.Error = "Issuer Base URL is required";
                return View("Index", model);
            }
            if (string.IsNullOrWhiteSpace(model.OAuthStrategyClientId))
            {
                model.Error = "Client ID is required";
                return View("Index", model);
            }
            if (string.IsNullOrWhiteSpace(model.OAuthStrategySecret))
            {
                model.Error = "Secret is required";
                return View("Index", model);
            }
            if (string.IsNullOrWhiteSpace(model.OAuthStrategyBaseUrl))
            {
                model.Error = "Base URL is required";
                return View("Index", model);
            }

            settings.OAuthStrategySecret = model.OAuthStrategySecret;
            settings.OAuthStrategyBaseUrl = model.OAuthStrategyBaseUrl;
            settings.OAuthStrategyClientId = model.OAuthStrategyClientId;
            settings.OAuthStrategyIssuerBaseUrl = model.OAuthStrategyIssuerBaseUrl;
        }

        SaveInitialData();
        
        if (model.Strategy == AuthStrategy.LocalStrategy)
        {
            var userService = new UserService();
            var user = userService.Register(model.LocalStrategyUsername, model.LocalStrategyPassword, isAdmin: true);

            // need to save service after registering the user to make correctly set InitDone to true
            service.Save();
            return Redirect("/");
        }
        else
        {
            service.Save();
            // need to restart the app to use OpenIDConnect
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                ApplicationLifetime.StopApplication();
            });
            return View("Restarting");
        }
    }


    /// <summary>
    /// Saves the initial data to the database
    /// </summary>
    private void SaveInitialData()
    {
        AddSearchEngine("DuckDuckGo", "duckduckgo.jpg", "ddg", "https://duckduckgo.com/?q=%s", isDefault: true);
        AddSearchEngine("Google", "google.png", "g", "https://www.google.com/search?q=%s");
        AddSearchEngine("YouTube", "youtube.png", "yt", "https://www.youtube.com/results?search_query=%s");
        AddSearchEngine("Ecosia", "ecosia.jpg", "ec", "https://www.ecosia.org/search?method=index&q=%s");
        CreateGuestDashboard();
    }

    /// <summary>
    /// Creates the default guest dashboard
    /// </summary>
    private void CreateGuestDashboard()
    {
        var uid = Globals.GuestDashbardUid;
        var dashboard = DbHelper.GetByUid<Dashboard>(uid);
        if (dashboard != null)
            return; // already exists
        var grpFenrusUid = new Guid("aa163b32-b9d7-4a1f-8737-e170091723e5");
        var grpFileFlowsUid = new Guid("87544728-5584-45ff-8e4b-fcbdbbd27994");
        
        CreateGroup(grpFenrusUid, "Fenrus", new List<GroupItem>()
        {
            new AppItem() { Uid  = new Guid("eed93c2d-9bdc-465a-84cc-78b8d47bdea4"), Name = "GitHub - Fenrus", Url = "https://github.com/revenz/Fenrus", Size = ItemSize.Large, AppName = "GitHub" },
            new AppItem() { Uid  = new Guid("33d584ec-8324-4525-8dcb-18d7fd06360f"), Name = "Discord - Fenrus", Url = "https://discord.gg/xbYK8wFMeU", Size = ItemSize.Medium, AppName = "Discord" },
            new AppItem() { Uid  = new Guid("86496260-3b31-481f-b5cc-98853cfde7bd"), Name = "Patreon - FileFlows", Url = "https://www.patreon.com/revenz", Size = ItemSize.Medium, AppName = "Patreon" },
        });
        CreateGroup(grpFileFlowsUid, "FileFlows", new List<GroupItem>()
        {
            new AppItem() { Uid  = new Guid("6e7cb2a0-8fe9-485b-9652-dc5e2efc8dce"), Name = "YouTube - FileFlows", Url = "https://www.youtube.com/watch?v=4Qu0y5Lem3c&list=PLGMTs-C_ZYk00lTsj6ghqftjykKDlWTDk", Size = ItemSize.Medium, AppName = "YouTube" },
            new AppItem() { Uid  = new Guid("72a0dc02-f263-4e2d-9f12-2cafab585200"), Name = "FileFlows Docs", Url = "https://docs.fileflows.com", Size = ItemSize.Medium, AppName = "Wiki" },
            new AppItem() { Uid  = new Guid("2b8b3159-60c6-40d3-b38f-3c8d8fc760f3"), Name = "GitHub - FileFlows", Url = "https://github.com/revenz/FileFlows", Size = ItemSize.Medium, AppName = "GitHub" },
        });
        
        dashboard = new();
        dashboard.Uid = uid;
        dashboard.Name = "Guest";
        dashboard.AccentColor = Globals.DefaultAccentColor;
        dashboard.BackgroundColor = Globals.DefaultBackgroundColor;;
        dashboard.ShowGroupTitles = true;
        dashboard.ShowSearch = true;
        dashboard.ShowStatusIndicators = false;
        dashboard.LinkTarget = "_self";
        dashboard.Background = "default.js";
        dashboard.Enabled = true;
        dashboard.Theme = "Default";
        dashboard.GroupUids ??= new();
        dashboard.GroupUids.Add(grpFenrusUid);
        dashboard.GroupUids.Add(grpFileFlowsUid);
        DbHelper.Insert(dashboard);
    }

    private Fenrus.Models.Group CreateGroup(Guid uid, string name, List<GroupItem> items)
    {
        var group = DbHelper.GetByUid<Fenrus.Models.Group>(uid);
        if (group != null)
            return group;
        group = new();
        group.Name = name;
        group.Uid = uid;
        group.Items = items;
        group.Enabled = true;
        group.IsSystem = true;
        DbHelper.Insert(group);
        return group;
    }

    /// <summary>
    /// Adds a search engine to the system if it doesn't already exist
    /// </summary>
    /// <param name="name">the name of the search engine</param>
    /// <param name="iconFile">the icon file on disk</param>
    /// <param name="shortcut">the shortcut to use for the search engine</param>
    /// <param name="url">the URL used for the search engine</param>
    /// <param name="isDefault">sets this search engine as the default</param>
    private void AddSearchEngine(string name, string iconFile, string shortcut, string url, bool isDefault = false)
    {
        var existing = DbHelper.GetByName<Models.SearchEngine>(name);
        if (existing != null)
            return; // already exists
        string fullFile = "wwwroot/search-engines/" + iconFile;
        if (System.IO.File.Exists(fullFile) == false)
            return; // cant add it
        var data = System.IO.File.ReadAllBytes(fullFile);
        string extension = iconFile.Substring(iconFile.LastIndexOf(".", StringComparison.Ordinal) + 1);
        var imageUid = ImageHelper.SaveImage(data, extension);
        DbHelper.Insert(new Models.SearchEngine()
        {
            Name = name,
            Icon = imageUid,
            Enabled = true,
            IsDefault = isDefault,
            IsSystem = true,
            Shortcut = shortcut,
            Url = url
        });
    }
}

/// <summary>
/// Model for the initial configuration
/// </summary>
public class InitConfigModel
{
    /// <summary>
    /// Gets or sets any error shown
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// Gets or sets the local strategy admin username
    /// </summary>
    public string LocalStrategyUsername { get; set; }
    
    /// <summary>
    /// Gets or sets the local strategy admin password
    /// </summary>
    public string LocalStrategyPassword { get; set; }
    
    /// <summary>
    /// Gets or sets if registrations are allowed
    /// </summary>
    public bool AllowRegister { get; set; }
    
    /// <summary>
    /// Gets or sets if guests are allowed
    /// </summary>
    public bool AllowGuest { get; set; }

    /// <summary>
    /// Gets or sets the OAuth strategy 
    /// </summary>
    public AuthStrategy Strategy { get; set; }

    /// <summary>
    /// Gets or sets the OAuth strategy issuer base url
    /// </summary>
    public string OAuthStrategyIssuerBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the OAuth strategy client id
    /// </summary>
    public string OAuthStrategyClientId { get; set; }

    /// <summary>
    /// Gets or sets the OAuth strategy secret
    /// </summary>
    public string OAuthStrategySecret { get; set; }

    /// <summary>
    /// Gets or sets the OAuth strategy base URL
    /// </summary>
    public string OAuthStrategyBaseUrl { get; set; }
}

/// <summary>
/// The login auth strategy
/// </summary>
public enum AuthStrategy
{
    /// <summary>
    /// Local strategy, using forms based authentication
    /// </summary>
    LocalStrategy = 0,
    /// <summary>
    /// OAuth strategy, using as single sign on provider
    /// </summary>
    OAuthStrategy = 1
}