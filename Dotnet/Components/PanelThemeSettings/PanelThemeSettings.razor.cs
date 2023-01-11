using Fenrus.Models;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Panel for the users theme settings
/// </summary>
public partial class PanelThemeSettings : ComponentBase
{
    private Theme Theme { get; set; }   
    private bool IsGuest { get; set; }
}