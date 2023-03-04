using Fenrus.Models;
using Microsoft.AspNetCore.Components.Routing;

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
        settings = new UserSettings();
        settings.Uid = uid;
        var guestDashboard = new DashboardService().GetGuestDashboard();
        Dashboard defaultDashboard;
        if (guestDashboard != null)
        {
            defaultDashboard = new Dashboard()
            {
                BackgroundColor = guestDashboard.BackgroundColor,
                Background = guestDashboard.Background,
                AccentColor = guestDashboard.AccentColor,
                ShowSearch = guestDashboard.ShowSearch,
                GroupUids = guestDashboard.GroupUids?.Select(x => x)?.ToList() ?? new(),
                LinkTarget = guestDashboard.LinkTarget,
                ShowGroupTitles = guestDashboard.ShowGroupTitles,
                ShowStatusIndicators = guestDashboard.ShowStatusIndicators,
                Theme = guestDashboard.Theme,
                BackgroundImage = guestDashboard.BackgroundImage,
            };
        }
        else
        {
            // create basic dashboard
            defaultDashboard= new ()
            {
                Uid = Guid.NewGuid(),
                AccentColor = "#FF0090",
                Background = "default.js",
                Theme = "Default",
                LinkTarget = "_self",
                ShowGroupTitles = true,
                ShowSearch = true,
                ShowStatusIndicators = true,
                BackgroundColor = "#009099",
                GroupUids = new ()
            };
        }

        defaultDashboard.Name = "Default";
        defaultDashboard.Uid = Guid.NewGuid();
        defaultDashboard.Enabled = true;
        if (string.IsNullOrWhiteSpace(guestDashboard.AccentColor)) guestDashboard.AccentColor = "#ff0090";
        if (string.IsNullOrWhiteSpace(guestDashboard.BackgroundColor)) guestDashboard.BackgroundColor = "#009099";
        if (string.IsNullOrWhiteSpace(guestDashboard.Background)) guestDashboard.Background = "default.js";
        if (string.IsNullOrWhiteSpace(guestDashboard.Theme)) guestDashboard.Theme = "Default";
        if (string.IsNullOrWhiteSpace(guestDashboard.LinkTarget)) guestDashboard.LinkTarget = "_self";

        if (defaultDashboard.GroupUids?.Any() != true)
            defaultDashboard.GroupUids = new GroupService().GetSystemGroups(enabledOnly: true).Select(x => x.Uid).ToList();
        settings.Dashboards = new List<Dashboard>()
        {
            defaultDashboard
        };

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