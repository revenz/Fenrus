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

    private string Target, OnClickCode, SerializedJsSafeModel;

    protected override void OnInitialized()
    {
        Target = Model.Target?.EmptyAsNull() ?? Settings.LinkTarget?.EmptyAsNull() ?? "_self";
        SerializedJsSafeModel = JsonSerializer.Serialize(Model).Replace("'", "\\'");
        OnClickCode = Target == "IFrame"
            ? $"openIframe(event, '{SerializedJsSafeModel}'); return false;"
            : $"launch(event, '{Model.Uid}')";
    }
}