using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// JavaScript version of Fenrus Switch
/// ALl events for value changes call javascript function
/// This is used on the main dashboard screen in the panel settings
/// </summary>
public partial class JavaScriptSwitch
{
    /// <summary>
    /// Gets or sets if this is checked
    /// </summary>
    [Parameter]
    public bool Value { get; set; }
    
    /// <summary>
    /// Gets or sets the code to execute when checked
    /// </summary>
    [Parameter]
    public string Code { get; set; }
}