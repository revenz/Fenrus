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
    /// Gets or sets the settings
    /// </summary>
    [Parameter] public UserSettings Settings { get; set; }

    /// <summary>
    /// Gets or sets the current dashboard
    /// </summary>
    [CascadingParameter] public Dashboard Dashboard { get; set; }

    private string Target, OnClickCode, SerializedJsSafeModel;

    protected override void OnInitialized()
    {
        Target = Model.Target?.EmptyAsNull() ?? Dashboard.LinkTarget?.EmptyAsNull() ?? "_self";
        SerializedJsSafeModel = JsonSerializer.Serialize(Model).Replace("'", "\\'");
        OnClickCode = Target == "IFrame"
            ? $"openIframe(event, '{SerializedJsSafeModel}'); return false;"
            : $"launch(event, '{Model.Uid}')";
    }

    /// <summary>
    /// Gets the Icon URL
    /// </summary>
    /// <returns>the Icon URL</returns>
    private string GetIcon()
    {
        if(string.IsNullOrWhiteSpace(Model.Icon))
            return "/favicon.svg?version=" + Globals.Version;
        if(Model.Icon.StartsWith("db:/image/"))
            return "/fimage/" + Model.Icon["db:/image/".Length..] + "?version=" + Globals.Version;
        return Model.Icon + "?version=" + Globals.Version;
    }
}