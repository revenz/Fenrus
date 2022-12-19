using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Service for user settings
/// </summary>
public class UserSettingsService
{
    private string FileForUser(Guid uid) => Path.Combine("data", "configs", uid + ".json");

    /// <summary>
    /// Gets the defaults settings for guests
    /// </summary>
    /// <returns></returns>
    public UserSettings SettingsForGuest()
    {
        var settings = new UserSettings()
        {
            Uid = Guid.Empty
        };
        return settings;
    }

    /// <summary>
    /// Loads a users settings
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public UserSettings? Load(Guid uid)
    {
        string file = FileForUser(uid);
        Logger.ILog($"checking for file: {file}");
        UserSettings? settings;
        if(File.Exists(file) == false)
        {
            // use default config
            var guest = SettingsForGuest();
            settings = new UserSettings();
            if (guest.Dashboards.Any())
            {
                settings.Dashboards.Add(guest.Dashboards.First());
                //settings.Dashboards.First().Name = "Default";
                //Logger.ILog("###################", guest.Dashboards[0].Name, self.Dashboards[0].Name);

                //self.Dashboards[0].Uid = new Utils().newGuid();
                //self.Dashboards[0].Enabled = true;
                //self.Dashboards[0].BackgroundImage = '';
                //settings.BackgroundImage = guest.Dashboards.First().BackgroundImage;
            }
            settings.Theme = "Default";
            settings.AccentColor = guest.AccentColor;

            Save(settings);
            return settings;
        }
        Logger.ILog("using config file: " + file);
        
        string json = File.ReadAllText(file);

        settings = JsonSerializer.Deserialize<UserSettings>(json);
        return settings;
    }

    /// <summary>
    /// Saves a users settings
    /// </summary>
    /// <param name="settings">the user settings</param>
    public void Save(UserSettings settings)
    {
        
    }
}