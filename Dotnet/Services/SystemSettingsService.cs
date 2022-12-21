using Fenrus.Controllers;
using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Service for system settings
/// </summary>
public class SystemSettingsService
{
    private static SystemSettings Settings;
    private static readonly string Filename = Path.Combine(DirectoryHelper.GetDataDirectory(), "system.json");
    /// <summary>
    /// Gets if init configuration is done
    /// </summary>
    public static bool InitConfigDone { get; private set; }

    /// <summary>
    /// Gets if using OAuth
    /// </summary>
    public static bool UsingOAuth => Settings?.Strategy == AuthStrategy.OAuthStrategy;

    static SystemSettingsService()
    {
        InitConfigDone = GetInitConfigDone();
        
        var settings = Load();
        if (settings != null)
        {
            Settings = settings;
            return;
        }
            
        // doesnt exist, or corrupt, lets create a new file
        settings = new();
        settings.AllowGuest = true;
        settings.AllowRegister = true;
        Settings = settings;
        SaveSettings();
    }

    private static void SaveSettings()
    {
        ++Settings.Revision;
        var existing = DbHelper.FirstOrDefault<SystemSettings>();
        if(existing == null)
            DbHelper.Insert(Settings);
        else
            DbHelper.Update(Settings);
        if (InitConfigDone == false)
            InitConfigDone = GetInitConfigDone();
    }

    private static SystemSettings? Load()
        => DbHelper.FirstOrDefault<SystemSettings>();

    /// <summary>
    /// Gets the system settings
    /// </summary>
    /// <returns>the system settings</returns>
    public SystemSettings Get()
        => Settings;

    /// <summary>
    /// Saves the current system settings
    /// </summary>
    public void Save()
        => SaveSettings();

    /// <summary>
    /// Gets if initial configuration is done
    /// </summary>
    /// <returns>if initial configuration is done</returns>
    private static bool GetInitConfigDone()
    {
        var settings = DbHelper.FirstOrDefault<SystemSettings>();
        if (settings == null)
            return false;
        
        // look for an admin user
        using var db = DbHelper.GetDb();
        var collection = db.GetCollection<Models.User>();
        collection.EnsureIndex(x => x.IsAdmin);
        var admins = collection.Query().Where(x => x.IsAdmin).Count();
        return admins > 0;
    }

    /// <summary>
    /// Marks the initial configuration as done
    /// </summary>
    public void MarkInitConfigDone()
        => InitConfigDone = true;
}