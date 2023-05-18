using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.Dialogs;

/// <summary>
/// Modal dialog
/// </summary>
public partial class Modal: ComponentBase
{
    /// <summary>
    /// Gets or sets the footer
    /// </summary>
    [Parameter] public RenderFragment Footer { get; set; }

    /// <summary>
    /// Gets or set the body
    /// </summary>
    [Parameter] public RenderFragment Body { get; set; }

    /// <summary>
    /// Gets or set the head
    /// </summary>
    [Parameter] public RenderFragment Head { get; set; }
    
    /// <summary>
    /// Gets or sets the title
    /// </summary>
    [Parameter] public string Title { get; set; }

    /// <summary>
    /// Gets or sets optional styling
    /// </summary>
    [Parameter] public string Styling { get; set; }
    
    /// <summary>
    /// Gets or sets if this is visible
    /// </summary>
    [Parameter] public bool Visible { get; set; }
}