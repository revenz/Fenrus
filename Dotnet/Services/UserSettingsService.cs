using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Service for user settings
/// </summary>
public class UserSettingsService
{
    // private string FileForUser(Guid uid) => Path.Combine("data", "configs", uid + ".json");

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
    /// <param name="uid">the UID of the user</param>
    /// <returns>the user settings</returns>
    public UserSettings? Load(Guid uid)
    {
        var settings = DbHelper.GetByUid<UserSettings>(uid);
        if (settings != null)
            return settings;
    
        // use default config
        var guest = SettingsForGuest();
        settings = new UserSettings();
        settings.Uid = uid;
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
        else
        {
            // create basic dashboard
            settings.Dashboards.Add(new ()
            {
                Uid = Guid.NewGuid(),
                AccentColor = guest.Dashboards?.FirstOrDefault()?.AccentColor?.EmptyAsNull() ?? "#FF0090",
                Background = "default.js",
                Enabled = true,
                Name = "Default",
                Theme = "Default",
                LinkTarget = "_self",
                ShowGroupTitles = true,
                ShowSearch = true,
                ShowStatusIndicators = true,
                BackgroundColor = "#000000",
                GroupUids = new ()
            });
        }

        DbHelper.Insert(settings);
        return settings;
    }

    /// <summary>
    /// Gets all the groups from every user
    /// This is used for the up time service 
    /// </summary>
    /// <returns>all the groups for all the users</returns>
    public List<Group> GetAllGroups()
    {
        var users = DbHelper.GetAll<UserSettings>();
        return users.SelectMany(x => x.Groups).ToList();
    }

    /// <summary>
    /// Saves a users settings
    /// </summary>
    /// <param name="settings">the user settings</param>
    public void Save(UserSettings settings)
        => DbHelper.Update(settings);

    /// <summary>
    /// Saves a background for a user
    /// </summary>
    /// <param name="uid">the Uid of the user</param>
    /// <param name="background">the background image data</param>
    /// <param name="extension">the background extension</param>
    public void SaveBackground(Guid uid, byte[] background, string extension)
    {
        
    }
}