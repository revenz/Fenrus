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
public class HomeController:Controller
{
    /// <summary>
    /// Gets the dashboard main page
    /// </summary>
    /// <returns>the dashboard main page</returns>
    [HttpGet]
    public IActionResult Home()
    {
#if(DEBUG)
        var settings = DemoHelper.GetDemoUserSettings();
        DashboardPageModel model = new DashboardPageModel
        {
            Dashboard = settings.Dashboards.First(),
            Settings = settings
        };
        return View("Dashboard", model);
#else
        Dashboard model = new Dashboard()
        {
            Name = "Test Dashboard"
        };
        return View("Dashboard", model);
#endif
    }

    /// <summary>
    /// Gets the login page
    /// </summary>
    /// <returns>the login page</returns>
    [HttpGet("login")]
    public IActionResult Login()
        => LoginPage(null);

    private IActionResult LoginPage(string error)
    {
        var settings = new Services.SystemSettingsService().Get();
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
        await CreateClaim(Guid.NewGuid(), username, true);
        return Redirect("/");
    }
    
    private async Task<IActionResult> Register(string username, string password)
    {
        var settings = new Services.SystemSettingsService().Get();
        if (settings.AllowRegister)
            return LoginPage("User registrations are not allowed");
        await CreateClaim(Guid.NewGuid(), username, true);
        return Redirect("/");
    }
    
    private async Task<IActionResult> Guest()
    {
        var settings = new Services.SystemSettingsService().Get();
        if (settings.AllowGuest == false)
            return LoginPage("Guest access is not allowed");
        
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