using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Up Time Componenent
/// </summary>
public partial class UpTimeComponent
{
    /// <summary>
    /// Gets or sets the Translater to use
    /// </summary>
    [Parameter] public Translater Translater { get; set; }
}