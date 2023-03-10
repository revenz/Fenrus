using Fenrus.Models;
using Microsoft.AspNetCore.Components.Routing;
using NLog;

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
                AccentColor = Globals.DefaultAccentColor,
                Background = "default.js",
                Theme = "Default",
                LinkTarget = "_self",
                ShowGroupTitles = true,
                ShowSearch = true,
                ShowStatusIndicators = true,
                BackgroundColor = Globals.DefaultBackgroundColor,
                GroupUids = new ()
            };
        }

        defaultDashboard.Name = "Default";
        defaultDashboard.Uid = Guid.NewGuid();
        defaultDashboard.UserUid = uid;
        defaultDashboard.Enabled = true;
        if (string.IsNullOrWhiteSpace(guestDashboard.AccentColor)) guestDashboard.AccentColor = Globals.DefaultAccentColor;
        if (string.IsNullOrWhiteSpace(guestDashboard.BackgroundColor)) guestDashboard.BackgroundColor = Globals.DefaultBackgroundColor;
        if (string.IsNullOrWhiteSpace(guestDashboard.Background)) guestDashboard.Background = "default.js";
        if (string.IsNullOrWhiteSpace(guestDashboard.Theme)) guestDashboard.Theme = "Default";
        if (string.IsNullOrWhiteSpace(guestDashboard.LinkTarget)) guestDashboard.LinkTarget = "_self";

        if (defaultDashboard.GroupUids?.Any() != true)
            defaultDashboard.GroupUids = new GroupService().GetSystemGroups(enabledOnly: true).Select(x => x.Uid).ToList();
        new DashboardService().Add(defaultDashboard);
        settings.DashboardUids = new List<Guid>()
        {
            defaultDashboard.Uid
        };

        DbHelper.Insert(settings);
        return settings;
    }

    /// <summary>
    /// Saves a users settings
    /// </summary>
    /// <param name="settings">the user settings</param>
    public void Save(UserSettings settings)
        => DbHelper.Update(settings);
        

    /// <summary>
    /// Adds users settings
    /// </summary>
    /// <param name="settings">the user settings to add</param>
    public void Add(UserSettings settings)
    {
        // delete it first in case it exists
        DbHelper.Delete<UserSettings>(settings.Uid);
        DbHelper.Insert(settings);
    }

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