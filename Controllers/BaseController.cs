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
        var uid = User.GetUserUid();
        if (uid == null)
            return null;
        
        var settings = new UserSettingsService().Load(uid.Value);
        return settings;
    }
    /// <summary>
    /// Gets the users profile for currently logged in user
    /// </summary>
    /// <returns>the user profile, or null if user not logged in</returns>
    protected UserProfile? GetUserProfile()
    {
        var uid = User.GetUserUid();
        if (uid == null)
            return null;
        
        var profile = new UserService().GetProfileByUid(uid.Value);
        return profile;
    }

    /// <summary>
    /// Gets the user uid
    /// </summary>
    /// <returns>the users UID, or null if couldn't be found</returns>
    protected Guid? GetUserUid() => User.GetUserUid();

    /// <summary>
    /// Gets a translator
    /// </summary>
    /// <param name="settings">the user settings</param>
    /// <returns>the translator</returns>
    protected Translator GetTranslator(UserSettings settings)
    {
        string language = settings.Language?.EmptyAsNull() ??
                          new SystemSettingsService().Get()?.Language?.EmptyAsNull() ?? "en";
        return Translator.GetForLanguage(language);
    }

    /// <summary>
    /// Gets the system settings
    /// </summary>
    /// <returns>the system settings</returns>
    public SystemSettings GetSystemSettings()
        => new SystemSettingsService().Get();
}