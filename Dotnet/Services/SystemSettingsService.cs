using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Service for system settings
/// </summary>
public class SystemSettingsService
{
    private static SystemSettings Settings;
    private static readonly string Filename = Path.Combine(DirectoryHelper.GetDataDirectory(), "system.json");

    static SystemSettingsService()
    {
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
        FileInfo fi = new FileInfo(Filename);
        if (fi.Directory.Exists == false)
            fi.Directory.Create();
        File.WriteAllText(Filename, JsonSerializer.Serialize(Settings));
    }

    private static SystemSettings? Load()
    {
        try
        {
            if (File.Exists(Filename) == false)
                return null;
            string json = File.ReadAllText(Filename);
            var settings = JsonSerializer.Deserialize<SystemSettings>(json);
            return settings;
        }
        catch (Exception)
        {
            return Settings;
        }
    }

    /// <summary>
    /// Gets the system settings
    /// </summary>
    /// <returns>the system settings</returns>
    public SystemSettings Get()
        => Settings;
}