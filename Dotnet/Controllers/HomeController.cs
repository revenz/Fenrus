using System.Security.Claims;
using Fenrus.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Home Controller
/// </summary>
[Route("/")]
public class HomeController : BaseController
{
    /// <summary>
    /// Gets the dashboard main page
    /// </summary>
    /// <returns>the dashboard main page</returns>
    [HttpGet]
    public IActionResult Home()
    {
        var settings = GetUserSettings();
        if(settings == null)
            return Redirect("/login");

        var dashboard = settings.Dashboards.FirstOrDefault() ?? new();
        if (string.IsNullOrEmpty(dashboard.BackgroundImage) == false)
            ViewBag.CustomBackground = "true";

        var themeService = new Services.ThemeService();
        var theme = themeService.GetTheme(dashboard.Theme?.EmptyAsNull() ?? "Default");
        var themes = themeService.GetThemes();
        
        var groups = dashboard.Groups.Select(x => settings.Groups.FirstOrDefault(y => y.Uid == x.Uid))
            .Where(x => x != null).ToList();
        
        DashboardPageModel model = new DashboardPageModel
        {
            Dashboard = dashboard,
            Settings = settings,
            Theme = theme,
            Groups = groups
        };
        ViewBag.IsGuest = false;
        ViewBag.Dashboard = dashboard;
        ViewBag.UserSettings = settings;
        ViewBag.Dashboards = settings.Dashboards;
        ViewBag.Themes = themes;
        ViewBag.Theme = theme;
        ViewBag.Accent = dashboard.AccentColor?.EmptyAsNull() ?? string.Empty;
        return View("Dashboard", model);
    }

    /// <summary>
    /// Gets a favicon
    /// </summary>
    /// <param name="color">the color of the favicon</param>
    /// <returns>the favicon</returns>
    [HttpGet("favicon")]
    [ResponseCache(Duration = 7 * 24 * 60 * 60)]
    public IActionResult Favicon([FromQuery] string color)
    {
        string svg = FavIconHelper.GetFavIconSvg(color);
        return Content(svg, "image/svg+xml");
    }

    /// <summary>
    /// Gets the login page
    /// </summary>
    /// <returns>the login page</returns>
    [HttpGet("login")]
    public IActionResult Login()
        => LoginPage(null);

    
    /// <summary>
    /// Logs the user out of the system
    /// </summary>
    /// <returns>the logout response</returns>
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/login");
    }

    /// <summary>
    /// Gets the login page
    /// </summary>
    /// <param name="error">[Optional] error to show on the login page</param>
    /// <returns>the login page response object</returns>
    private IActionResult LoginPage(string error)
    {
        var settings = GetSystemSettings();
        LoginPageModel model = new LoginPageModel()
        {
            Error = error,
            AllowGuest = settings.AllowGuest,
            AllowRegister = settings.AllowRegister
        };
        return View("Login", model);
    }

    /// <summary>
    /// Login post form request
    /// </summary>
    /// <param name="action">the action being performed, Login, Register, or Guest</param>
    /// <param name="username">the username</param>
    /// <param name="password">the password</param>
    /// <returns>the resulting action result</returns>
    [HttpPost("login")]
    public async Task<IActionResult> LoginPost([FromForm] string action, [FromForm] string username, [FromForm] string password)
    {
        Console.WriteLine("Action: " + action);
        Console.WriteLine("Username: " + username);
        Console.WriteLine("Password: " + password);

        switch (action)
        {
            case "Login":
                return await Login(username, password);
            case "Register":
                return await Register(username, password);
            case "Guest":
                return await Guest();
        }

        return Login();
    }


    private async Task<IActionResult> Login(string username, string password)
    {
        var user = new Services.UserService().Validate(username, password);
        if (user == null)
            return LoginPage("Login failed");
        
        await CreateClaim(user.Uid, user.Name, user.IsAdmin);
        return Redirect("/");
    }
    
    private async Task<IActionResult> Register(string username, string password)
    {
        var settings = GetSystemSettings();
        if (settings.AllowRegister == false)
            return LoginPage("User registrations are not allowed");
        var user = new Services.UserService().Register(username, password);
        await CreateClaim(user.Uid, user.Name, user.IsAdmin);
        return Redirect("/");
    }
    
    private async Task<IActionResult> Guest()
    {
        var settings = GetSystemSettings();
        if (settings.AllowGuest == false)
            return LoginPage("Guest access is not allowed");

        ViewBag.IsGuest = true;
        throw new NotImplementedException();
    }

    private async Task CreateClaim(Guid uid, string username, bool isAdmin)
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, username),
            new (ClaimTypes.Sid, uid.ToString()),
        };
        if (isAdmin)
            claims.Add(new(ClaimTypes.Role, "Administrator"));

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            //AllowRefresh = <bool>,
            // Refreshing the authentication session should be allowed.

            //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
            // The time at which the authentication ticket expires. A 
            // value set here overrides the ExpireTimeSpan option of 
            // CookieAuthenticationOptions set with AddCookie.

            //IsPersistent = true,
            // Whether the authentication session is persisted across 
            // multiple requests. When used with cookies, controls
            // whether the cookie's lifetime is absolute (matching the
            // lifetime of the authentication ticket) or session-based.

            //IssuedUtc = <DateTimeOffset>,
            // The time at which the authentication ticket was issued.

            //RedirectUri = <string>
            // The full path or absolute URI to be used as an http 
            // redirect response value.
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(claimsIdentity), 
            authProperties);
    }
}

/// <summary>
/// The model for the login page
/// </summary>
public class LoginPageModel
{
    /// <summary>
    /// Gets or sets if there is an error to show
    /// </summary>
    public string Error { get; set; }
    /// <summary>
    /// Gets or sets if registrations are allowed
    /// </summary>
    public bool AllowRegister { get; set; }
    /// <summary>
    /// Gets or sets if guests are allowed
    /// </summary>
    public bool AllowGuest { get; set; }
}