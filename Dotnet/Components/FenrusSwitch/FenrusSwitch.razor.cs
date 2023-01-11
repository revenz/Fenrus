using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Switch/Toggle control
/// </summary>
public partial class FenrusSwitch
{
    
    private bool _Value;
    
    /// <summary>
    /// Gets or sets the value of the switch (on or off)
    /// </summary>
    [Parameter]
    public bool Value
    {
        get => _Value;
        set
        {
            if(_Value != value)
            {
                _Value = value;
                if(ValueChanged.HasDelegate)
                    ValueChanged.InvokeAsync(value);
            }
        }
    }
    
    /// <summary>
    /// Gets or sets if this input is disabled
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets an event that is called when the value changes
    /// </summary>
    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; }

    /// <summary>
    /// Event that is fired when the value is changed
    /// </summary>
    /// <param name="args">the change arguments</param>
    private void OnChange(ChangeEventArgs args)
    {
        if (Disabled)
            return;
        this.Value = args.Value as bool? == true;
    }

    /// <summary>
    /// Toggles the switches value/state
    /// </summary>
    /// <param name="args">the change arguments</param>
    private void ToggleValue(EventArgs args)
    {
        if (Disabled)
            return;
        this.Value = !this.Value;
    }
}