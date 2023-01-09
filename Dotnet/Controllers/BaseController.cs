using Fenrus.Models;
using Fenrus.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Base Controller used by Fenrus
/// </summary>
public class BaseController : Controller
{
    /// <summary>
    /// Gets the users settings for currently logged in user
    /// </summary>
    /// <returns>the user settings, or null if user not logged in</returns>
    protected UserSettings? GetUserSettings()
    {
        var sid = User?.Claims?.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid")?.Value;
        if (string.IsNullOrEmpty(sid) || Guid.TryParse(sid, out Guid uid) == false)
            return null;
        
        var settings = new Services.UserSettingsService().Load(uid);
        return settings;
    }

    /// <summary>
    /// Gets the system settings
    /// </summary>
    /// <returns>the system settings</returns>
    public SystemSettings GetSystemSettings()
        => new SystemSettingsService().Get();
}