using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.Inputs;

/// <summary>
/// Input Number
/// </summary>
public partial class InputNumber<TItem> : Input<TItem>
{
    /// <summary>
    /// Gets or sets a maximum value
    /// </summary>
    [Parameter]
    public TItem? Maximum { get; set; }

    /// <summary>
    /// Gets or sets a minimum value
    /// </summary>
    [Parameter]
    public TItem? Minimum { get; set; }
}