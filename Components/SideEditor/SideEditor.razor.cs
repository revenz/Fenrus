using Fenrus.Components.Inputs;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Side editor
/// </summary>
public partial class SideEditor:EditorForm
{
    /// <summary>
    /// Gets or sets the icon to display
    /// </summary>
    [Parameter] public string Icon { get; set; }
    /// <summary>
    /// Gets or sets the title to show
    /// </summary>
    [Parameter] public string Title { get; set; }

    /// <summary>
    /// Gets or sets the top buttons
    /// </summary>
    [Parameter] public RenderFragment Buttons { get; set; }

    /// <summary>
    /// Gets or sets the main page content
    /// </summary>
    [Parameter] public RenderFragment Body { get; set; }
}