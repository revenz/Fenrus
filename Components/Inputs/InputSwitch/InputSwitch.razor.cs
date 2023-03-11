using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.Inputs;

/// <summary>
/// Input Switch
/// </summary>
public partial class InputSwitch : Input<bool>
{
    /// <summary>
    /// Gets or sets if the value should be inverted
    /// </summary>
    [Parameter] public bool Invert { get; set; }

    void ToggleValue()
    {
        Console.WriteLine("Toggling value");
        this.Value = !this.Value;
        this.StateHasChanged();
    }
}