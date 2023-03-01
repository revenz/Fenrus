using Fenrus.Models;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Panel for user settings
/// </summary>
public partial class PanelUserSettings : ComponentBase
{
    /// <summary>
    /// Gets or sets the dashboards in the system
    /// </summary>
    [Parameter] public List<Dashboard> Dashboards { get; set; }

    /// <summary>
    /// Gets or sets the user settings
    /// </summary>
    [Parameter] public UserSettings Settings { get; set; }
    
    /// <summary>
    /// Gets or sets the current dashboard
    /// </summary>
    [Parameter] public Dashboard Dashboard { get; set; }

    /// <summary>
    /// Gets or sets a list of all themes in the system
    /// </summary>
    [Parameter] public List<string> Themes { get; set; }
    
    /// <summary>
    /// Gets or set the current theme
    /// </summary>
    [Parameter] public Theme Theme { get; set; }

    /// <summary>
    /// Gets or sets the page helper
    /// </summary>
    [Parameter] public PageHelper PageHelper { get; set; }
}