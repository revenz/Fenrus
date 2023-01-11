using Fenrus.Models;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Panel for the users theme settings
/// </summary>
public partial class PanelThemeSettings : ComponentBase
{
    /// <summary>
    /// Gets or sets the theme the user is using
    /// </summary>
    [Parameter]
    public Theme Theme { get; set; }   
    
    /// <summary>
    /// Gets or sets if the current user is a guest or logged in user
    /// </summary>
    [Parameter]
    public bool IsGuest { get; set; }
}