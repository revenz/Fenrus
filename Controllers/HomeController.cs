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
    private Translator Translator;

    /// <summary>
    /// In memory password reset tokens
    /// </summary>
    private static readonly List<PasswordResetModel> ResetTokens = new();

    private const string MSG_PasswordResetTokenInvalid = "PasswordResetTokenInvalid";
    private const string MSG_PasswordResetFailed = "PasswordResetFailed";
    private const string MSG_PasswordReset = "PasswordReset";
    
    public HomeController()
    {
        var language = new SystemSettingsService().Get()?.Language;
        if (string.IsNullOrWhiteSpace(language))
            language = "en";
        Translator = Translator.GetForLanguage(language);
    }

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

        if (string.IsNullOrWhiteSpace(settings.Language) == false)
            Translator = Helpers.Translator.GetForLanguage(settings.Language); 

        var dashboard = settings.Dashboards.FirstOrDefault() ?? new();
        return ShowDashboard(dashboard, settings);
    }

    private IActionResult ShowDashboard(Dashboard dashboard, UserSettings settings)
    {
        if (string.IsNullOrEmpty(dashboard.BackgroundImage) == false)
            ViewBag.CustomBackground = "true";

        var themeService = new Services.ThemeService();
        var theme = themeService.GetTheme(dashboard.Theme?.EmptyAsNull() ?? "Default");
        var themes = themeService.GetThemes();
        
        // load groups form the dashboard uids
        // should move this into a service
        var systemGroups = DbHelper.GetAll<Models.Group>().Where(x => x.Enabled && x.IsSystem)
            .DistinctBy(x => x.Uid)
            .ToDictionary(x => x.Uid, x => x);
        var groups = new List<Models.Group>();
        foreach (var gUid in dashboard.GroupUids)
        {
            var grp = settings.Groups.FirstOrDefault(x => x.Uid == gUid);
            if (grp != null)
            {
                if (grp.Enabled)
                    groups.Add(grp);
            }
            else if (systemGroups.ContainsKey(gUid))
            {
                groups.Add(systemGroups[gUid]);
            }
        }
        
        DashboardPageModel model = new DashboardPageModel
        {
            Dashboard = dashboard,
            Settings = settings,
            Theme = theme,
            Groups = groups,
            Translator = Translator
        };
        ViewBag.Translator = Translator;
        ViewBag.IsGuest = false;
        ViewBag.Dashboard = dashboard;
        ViewBag.UserSettings = settings;
        ViewBag.Dashboards = settings.Dashboards;
        ViewBag.Themes = themes;
        ViewBag.Theme = theme;
        ViewBag.SystemSearchEngines = DbHelper.GetAll<Models.SearchEngine>();
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
    /// <param name="msg">[Optional] message to show on the login page</param>
    /// <returns>the login page</returns>
    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? msg = null)
    {
        if (SystemSettingsService.InitConfigDone == false)
            return Redirect("/init-config");

        if (SystemSettingsService.UsingOAuth)
            return Redirect("/sso");
        
        if (msg == MSG_PasswordResetTokenInvalid)
            return LoginPage(Translator.Instant("Pages.Login.Labels.PasswordResetTokenInvalid"));
        if (msg == MSG_PasswordReset)
            return LoginPage(Translator.Instant("Pages.Login.Labels.PasswordReset"));
        if (msg == MSG_PasswordResetFailed)
            return LoginPage(Translator.Instant("Pages.Login.Labels.PasswordResetFailed"));
        return LoginPage(null);
    }


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
    private IActionResult LoginPage(string? error)
    {
        var settings = GetSystemSettings();
        var guestDashboard = new DashboardService().GetGuestDashboard();
        ViewBag.Translator = Translator;
        LoginPageModel model = new LoginPageModel()
        {
            Error = error,
            AllowGuest = settings.AllowGuest,
            AllowRegister = settings.AllowRegister,
            Translator = Translator,
            BackgroundColor = guestDashboard?.BackgroundColor?.EmptyAsNull() ?? Globals.DefaultBackgroundColor,
            AccentColor = guestDashboard?.AccentColor?.EmptyAsNull() ?? Globals.DefaultAccentColor
        };
        return View("Login", model);
    }

    /// <summary>
    /// Login post form request
    /// </summary>
    /// <param name="action">the action being performed, Login, Register, or Guest</param>
    /// <param name="username">the username</param>
    /// <param name="password">the password</param>
    /// <param name="usernameOrEmail">the username or email for a password reset</param>
    /// <returns>the resulting action result</returns>
    [HttpPost("login")]
    public async Task<IActionResult> LoginPost([FromForm] string action, [FromForm] string username, [FromForm] string password, [FromForm] string usernameOrEmail)
    {
        Logger.ILog($"Login[{action}]: {username}");

        if (action == "Reset")
            return Reset(usernameOrEmail);

        if(action == "Login")
            return await Login(username, password);
        if(action == "Register")
            return await Register(username, password);
        return Login();
    }


    private async Task<IActionResult> Login(string username, string password)
    {
        var user = new Services.UserService().Validate(username, password);
        if (user == null)
            return LoginPage(Translator.Instant("Pages.Login.ErrorMessages.LoginFailed"));
        
        await CreateClaim(user.Uid, user.Name, user.IsAdmin);
        return Redirect("/");
    }
    
    private async Task<IActionResult> Register(string username, string password)
    {
        var settings = GetSystemSettings();
        if (settings.AllowRegister == false)
            return LoginPage(Translator.Instant("Pages.Login.ErrorMessages.RegistrationNotAllowed"));
        var user = new Services.UserService().Register(username, password);
        await CreateClaim(user.Uid, user.Name, user.IsAdmin);
        return Redirect("/");
    }

    private IActionResult Reset(string usernameOrEmail)
    {
        var user = new UserService().FindUser(usernameOrEmail);
        if (user != null)
        {
            Guid token = Guid.NewGuid();
            ResetTokens.Add(new ()
            {
                Date = DateTime.Now,
                Token = token,
                UserUid = user.Uid
            });
            string resetUrl = HttpContext.Request.Scheme + "://" +
                              HttpContext.Request.Host + 
                              HttpContext.Request.Path;
            resetUrl = resetUrl.Substring(0, resetUrl.IndexOf("/login"));
            resetUrl += "/reset-password/" + token;

            if (string.IsNullOrEmpty(user.Email) == false)
            {
                Emailer.Send(
                    recipient: user.Email,
                    subject: "Fenrus Password Reset",
                    plainTextBody: "Here is your password reset link from Fenrus\n\n" + resetUrl
                );
            }

            Console.WriteLine("Password Reset Url: " + resetUrl);
        }
        
        return LoginPage(Translator.Instant("Pages.Login.Labels.PasswordResetSent"));
    }

    /// <summary>
    /// Performs a password reset
    /// </summary>
    /// <param name="token">the reset token</param>
    /// <returns>the password reset page</returns>
    [HttpGet("reset-password/{token}")]
    public IActionResult PasswordReset([FromRoute] Guid token)
    {
        var reset = ResetTokens.FirstOrDefault(x => x.Token == token
                                                    && x.Date > DateTime.Now.AddMinutes(-10));

        // remove the token
        ResetTokens.RemoveAll(x => x.Token == token);
        
        if (reset == null)
            return Redirect("/login?msg=" + MSG_PasswordResetTokenInvalid);

        var newPassword = Helpers.PasswordGenerator.Generate(20, 4);
        var service = new UserService();
        var changed = service.ChangePassword(reset.UserUid, newPassword);

        if (changed == false)
            return Redirect("/login?msg=" + MSG_PasswordResetFailed);

        var user = service.GetByUid(reset.UserUid);
        
        if (string.IsNullOrEmpty(user?.Email) == false)
        {
            Emailer.Send(
                recipient: user.Email,
                subject: "New Fenrus Password",
                plainTextBody: "Here is your password for Fenrus\n\n" + newPassword + "\n\nYou should change this immediately after logging in."
            );
        }
        Console.WriteLine("New Password: " + newPassword);
        return Redirect("/login?msg=" + MSG_PasswordReset);
    }
    
    /// <summary>
    /// The guest dashboard
    /// </summary>
    /// <returns>the guest dashboard response</returns>
    [HttpGet("guest")]
    public async Task<IActionResult> Guest()
    {
        var settings = GetSystemSettings();
        if (settings.AllowGuest == false)
            return Redirect("/login");

        ViewBag.IsGuest = true;
        var dashboard = new DashboardService().GetGuestDashboard();

        return ShowDashboard(dashboard, new UserSettingsService().SettingsForGuest());
    }

    /// <summary>
    /// Request an OAuth login
    /// </summary>
    [HttpGet("sso")]
    [Authorize]
    public IActionResult OAuthLogin()
    {
        var settings = GetSystemSettings();
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

    /// <summary>
    /// Password reset model
    /// </summary>
    private class PasswordResetModel
    {
        /// <summary>
        /// Gets or sets the date when this token was created
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the token
        /// </summary>
        public Guid Token { get; set; }
        
        /// <summary>
        /// Gets or sets the user UID 
        /// </summary>
        public Guid UserUid { get; set; }
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
    public string? Error { get; set; }
    /// <summary>
    /// Gets or sets if registrations are allowed
    /// </summary>
    public bool AllowRegister { get; set; }
    /// <summary>
    /// Gets or sets if guests are allowed
    /// </summary>
    public bool AllowGuest { get; set; }
    
    /// <summary>
    /// Gets the translator to use for the page
    /// </summary>
    public Translator Translator { get; init; }

    /// <summary>
    /// Gets or sets the background color
    /// </summary>
    public string BackgroundColor { get; set; }
    
    /// <summary>
    /// Gets or sets the accent color
    /// </summary>
    public string AccentColor { get; set; }
}