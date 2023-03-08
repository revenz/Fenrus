using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.SideEditors;

public class SideEditorBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the translator to use
    /// </summary>
    [Parameter] public Translator Translator { get; set; }
    
}