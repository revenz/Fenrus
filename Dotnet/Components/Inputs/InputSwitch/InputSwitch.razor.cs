namespace Fenrus.Components.Inputs;

/// <summary>
/// Input Switch
/// </summary>
public partial class InputSwitch : Input<bool>
{
    void ToggleValue()
    {
        Console.WriteLine("Toggling value");
        this.Value = !this.Value;
        this.StateHasChanged();
    }
}