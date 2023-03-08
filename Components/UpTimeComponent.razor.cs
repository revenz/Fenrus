using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Up Time Componenent
/// </summary>
public partial class UpTimeComponent
{
    /// <summary>
    /// Gets or sets the Translator to use
    /// </summary>
    [Parameter] public Translator Translator { get; set; }
}