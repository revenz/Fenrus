using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.Inputs;

/// <summary>
/// Input label
/// </summary>
public partial class InputLabel : Input<string>
{
    /// <summary>
    /// Gets or sets if the value is a link
    /// </summary>
    [Parameter]
    public bool Link { get; set; }
    
    /// <summary>
    /// Gets or sets the link target
    /// </summary>
    [Parameter]
    public string Target { get; set; }
}