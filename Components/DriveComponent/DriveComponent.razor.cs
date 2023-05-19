using Fenrus.Models;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Component that shows the users notes
/// </summary>
public partial class DriveComponent
{
    /// <summary>
    /// Gets or sets the Translator to use
    /// </summary>
    [Parameter] public Translator Translator { get; set; }
    
    /// <summary>
    /// Gets or sets the users UID
    /// </summary>
    [Parameter] public Guid UserUid { get; set; }
    
    /// <summary>
    /// Gets or sets the system settings
    /// </summary>
    [Parameter] public SystemSettings SystemSettings { get; set; }
    
    /// <summary>
    /// Gets or sets the user profile
    /// </summary>
    [Parameter] public UserProfile UserProfile { get; set; }
    
    /// <summary>
    /// Gets or sets the accent colour
    /// </summary>
    [Parameter] public string AccentColor { get; set; }

    private string lblTitle, lblNotes, lblPersonal, lblDashboard, lblShared, lblFiles, lblCalendar, lblEmail;
    private bool NotesEnabled, FilesEnabled, AppsEnabled, CalendarEnabled, EmailEnabled;
    
    /// <summary>
    /// Checks if a cloud feature is enabled
    /// </summary>
    /// <param name="feature">the feature to check</param>
    /// <returns>true if enable, otherwise false</returns>
    private bool FeatureEnable(CloudFeature feature)
    {
        if ((SystemSettings.CloudFeatures & feature) != feature)
            return false;

        return (UserProfile.CloudFeatures & feature) == feature;
    }

    protected override void OnInitialized()
    {
        this.lblTitle = Translator.Instant("Labels.Notes");
        this.lblNotes = Translator.Instant("Labels.Notes");
        this.lblPersonal = Translator.Instant("Labels.Personal");
        this.lblDashboard = Translator.Instant("Labels.Dashboard");
        this.lblShared = Translator.Instant("Labels.Shared");
        this.lblFiles = Translator.Instant("Labels.Files");
        this.lblCalendar = Translator.Instant("Labels.Calendar");
        this.lblEmail = Translator.Instant("Labels.Email");

        this.NotesEnabled = FeatureEnable(CloudFeature.Notes);
        this.AppsEnabled = FeatureEnable(CloudFeature.Apps);
        this.CalendarEnabled = FeatureEnable(CloudFeature.Calendar);
        this.FilesEnabled = FeatureEnable(CloudFeature.Files);
        this.EmailEnabled = FeatureEnable(CloudFeature.Email);
    }
    
    
    /// <summary>
    /// Gets the Icon URL
    /// </summary>
    /// <returns>the Icon URL</returns>
    private string GetIcon(GroupItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Icon) != true)
        {
            if (item.Icon.StartsWith("db:/image/"))
                return "/fimage/" + item.Icon["db:/image/".Length..] + "?version=" + Globals.Version;
            return item.Icon + "?version=" + Globals.Version;
        }
        
        if (item is AppItem appItem == false)
            return $"/favicon?color={AccentColor?.Replace("#", string.Empty)}&version={Globals.Version}";

        var app = AppService.GetByName(appItem.AppName);

        if (string.IsNullOrEmpty(app.Icon) == false)
            return $"/apps/{Uri.EscapeDataString(app.Name)}/{app.Icon}?version=" + Globals.Version;
        return $"/favicon?color={AccentColor?.Replace("#", string.Empty)}&version={Globals.Version}";
    }

    /// <summary>
    /// Gets the address for a app
    /// </summary>
    /// <param name="app">the app</param>
    /// <returns>the address to put into the HTML</returns>
    private string GetAddress(CloudApp app)
    {
        if (app.Type != CloudAppType.Ssh)
            return app.Address;
        string address = app.Address;
        int atIndex = address.IndexOf("@");
        if (atIndex < 0)
            return address;
        string user = address[0..atIndex];
        address = address.Substring(atIndex + 1);
        int colonIndex = user.IndexOf(":");
        if (colonIndex < 0)
            return app.Uid + "@" + address;
        // so we never ever every not in a million years send the password to the client
        return app.Uid + ":" + app.Uid + "@" + address;
    }
}