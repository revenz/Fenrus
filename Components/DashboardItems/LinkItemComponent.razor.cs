using Fenrus.Models;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.DashboardItems;

/// <summary>
/// Link item component
/// </summary>
public partial class LinkItemComponent
{
    
    /// <summary>
    /// Gets or sets the link item model
    /// </summary>
    [Parameter] public LinkItem Model { get; set; }

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
    
    /// <summary>
    /// Gets the up-time states of URLs
    /// </summary>
    [Parameter] public Dictionary<string, int> UpTimeStates { get; init; }

    private string Target, OnClickCode, SerializedJsSafeModel, Css;

    protected override void OnInitialized()
    {
        Target = Model.Target?.EmptyAsNull() ?? Dashboard.LinkTarget?.EmptyAsNull() ?? "_self";
        SerializedJsSafeModel = JsonSerializer.Serialize(Model).Replace("'", "\\'");
        OnClickCode = Target == "IFrame"
            ? $"openIframe(event, '{SerializedJsSafeModel}'); return false;"
            : $"launch(event, '{Model.Uid}')";

        Css = string.Empty;
        if (Model.Monitor && string.IsNullOrWhiteSpace(Model.Url) == false && UpTimeStates?.TryGetValue(Model.Url, out int state) == true)
            Css += "is-up-" + (state == 0 ? "false" : state == 1 ? "true" : state) + " ";
    }

    /// <summary>
    /// Gets the Icon URL
    /// </summary>
    /// <returns>the Icon URL</returns>
    private string GetIcon()
    {
        if(string.IsNullOrWhiteSpace(Model.Icon))
            return $"/favicon?color={@Uri.EscapeDataString(Dashboard.AccentColor)}&version={Globals.Version}";
        if(Model.Icon.StartsWith("db:/image/"))
            return "/fimage/" + Model.Icon["db:/image/".Length..] + "?version=" + Globals.Version;
        return Model.Icon + "?version=" + Globals.Version;
    }
}