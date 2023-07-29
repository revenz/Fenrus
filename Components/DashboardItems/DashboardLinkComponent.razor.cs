using Fenrus.Models;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.DashboardItems;

/// <summary>
/// Dashboard link component
/// </summary>
public partial class DashboardLinkComponent
{
    
    /// <summary>
    /// Gets or sets the link item model
    /// </summary>
    [Parameter] public DashboardLinkItem Model { get; set; }

    /// <summary>
    /// Gets or sets if this is a guest dashboard
    /// </summary>
    [Parameter] public bool IsGuest { get; set; }
    
    /// <summary>
    /// Gets or sets the settings
    /// </summary>
    [Parameter] public UserSettings Settings { get; set; }

    /// <summary>
    /// Gets or sets the current dashboard
    /// </summary>
    [CascadingParameter] public Dashboard Dashboard { get; set; }

    private Dashboard TargetDashboard;
    private string SerializedJsSafeModel;
    

    protected override void OnInitialized()
    {
        SerializedJsSafeModel = JsonSerializer.Serialize(Model).Replace("'", "\\'");
        var db = new DashboardService().GetByUid(Model.DashboardUid);
        if (db == null)
            return;
        if (db.UserUid != Settings.UserUid && db.IsDefault == false)
            return;
        TargetDashboard = db;
    }

    /// <summary>
    /// Gets the Icon URL
    /// </summary>
    /// <returns>the Icon URL</returns>
    private string GetIcon()
    {
        if(string.IsNullOrWhiteSpace(Model.Icon))
            return $"/favicon?color={Dashboard.AccentColor?.Replace("#", string.Empty)}&version={Globals.Version}";
        if(Model.Icon.StartsWith("db:/image/"))
            return "/fimage/" + Model.Icon["db:/image/".Length..] + "?version=" + Globals.Version;
        return Model.Icon + "?version=" + Globals.Version;
    }
}