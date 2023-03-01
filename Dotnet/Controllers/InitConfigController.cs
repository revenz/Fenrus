using Fenrus.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using IApplicationLifetime = Microsoft.Extensions.Hosting.IApplicationLifetime;

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
    private IApplicationLifetime ApplicationLifetime { get; set; }
    
    /// <summary>
    /// Constructs an instance of the init config controller
    /// </summary>
    /// <param name="appLifetime">the application lifetime used to restart the application</param>
    public InitConfigController(IApplicationLifetime appLifetime)
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
        string url = HttpContext.Request.GetDisplayUrl();
        url = url[..(url.IndexOf('/', 8) + 1)];
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
        var service = new SystemSettingsService();
        var settings = service.Get();
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
                await Task.Delay(250);
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