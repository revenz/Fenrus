using Fenrus.Models;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.DashboardItems;

/// <summary>
/// Terminal item component
/// </summary>
public partial class TerminalItemComponent
{
    
    /// <summary>
    /// Gets or sets the terminal item model
    /// </summary>
    [Parameter] public TerminalItem Model { get; set; }
    
    /// <summary>
    /// Gets or sets the settings
    /// </summary>
    [Parameter] public UserSettings Settings { get; set; }

    /// <summary>
    /// Gets or sets the current dashboard
    /// </summary>
    [CascadingParameter] public Dashboard Dashboard { get; set; }

    private string? Target;
    private string OnClickCode, SerializedJsSafeModel;

    protected override void OnInitialized()
    {
        SerializedJsSafeModel = JsonSerializer.Serialize(Model).Replace("'", "\\'");

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