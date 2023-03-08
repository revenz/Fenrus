using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Editor form to be used inline
/// </summary>
public partial class Editor:EditorForm
{
    /// <summary>
    /// Gets or sets the child content
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; }
}