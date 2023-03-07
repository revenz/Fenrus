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
        
        SystemSettings settings;
        if (InitConfigDone)
        {
            settings = Load();
            if (settings != null)
            {
                Settings = settings;
                return;
            }
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
        if (File.Exists(DbHelper.DbFile) == false)
        {
            Logger.ILog("GetInitConfigDone(): DbFile Does not exist: " + DbHelper.DbFile);
            return false;
        }

        var settings = DbHelper.FirstOrDefault<SystemSettings>();
        if (settings == null)
        {
            Logger.ILog("GetInitConfigDone(): SystemSettings not saved in database");
            return false;
        }

        // look for an admin user
        using var db = DbHelper.GetDb();
        var collection = db.GetCollection<Models.User>();
        collection.EnsureIndex(x => x.IsAdmin);
        var admins = collection.Query().Where(x => x.IsAdmin).Count();
        bool hasAdmins = admins > 0;
        if (hasAdmins)
            return true;
        
        // check if they are using SSO, if they are and theres no admins, we say initial config is done so they
        // can register the admin
        if (settings.Strategy == AuthStrategy.OAuthStrategy)
        {
            Logger.ILog("GetInitConfigDone(): No Admins but using OAuth");
            return true;
        }

        Logger.ILog("GetInitConfigDone(): No Admins in database and using Strategy: " + settings.Strategy);
        return false;
    }

    /// <summary>
    /// Marks the initial configuration as done
    /// </summary>
    public void MarkInitConfigDone()
        => InitConfigDone = true;
}