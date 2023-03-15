using Fenrus.Models;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Main dashboard component
/// </summary>
public partial class DashboardComponent
{
    /// <summary>
    /// Gets or sets the user settings
    /// </summary>
    [Parameter] public UserSettings Settings { get; set; }
    
    /// <summary>
    /// Gets or sets the page helper
    /// </summary>
    [Parameter] public PageHelper PageHelper { get; set; }

    /// <summary>
    /// Gets or sets the dashboard model
    /// </summary>
    [Parameter] public Dashboard Model { get; set; }

    /// <summary>
    /// Gets or sets the groups on this dashboard
    /// </summary>
    [Parameter] public List<Group> Groups { get; set; }

    /// <summary>
    /// Gets or sets the Translator to use
    /// </summary>
    [Parameter] public Translator Translator { get; set; }
    
    /// <summary>
    /// Gets the up-time states of URLs
    /// </summary>
    [Parameter] public Dictionary<string, int> UpTimeStates { get; init; }
}