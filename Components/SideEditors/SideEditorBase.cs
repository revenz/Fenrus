using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.SideEditors;

public class SideEditorBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the translater to use
    /// </summary>
    [Parameter] public Translater Translater { get; set; }
    
}