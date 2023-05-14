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
    /// Gets or sets the accent colour
    /// </summary>
    [Parameter] public string AccentColor { get; set; }

    private string lblTitle, lblNotes, lblPersonal, lblDashboard, lblShared, lblFiles, lblCalendar, lblEmail;
    private Group? AppDrawerGroup;

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

        if (UserUid != Guid.Empty)
        {
            AppDrawerGroup = new GroupService().GetAllForUser(UserUid).FirstOrDefault(x => x.Name == "Drive");
        }
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
}