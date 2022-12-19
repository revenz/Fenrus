using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

public partial class FenrusSwitch
{
    
    private bool _Value;
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

    [Parameter]
    public EventCallback<bool> ValueChanged{get;set; }

    [Parameter]
    public Expression<Func<bool>> ValueExpression { get; set; }

    private void OnChange(ChangeEventArgs args)
    {
        this.Value = args.Value as bool? == true;
    }

    private void ToggleValue(EventArgs args)
    {
        this.Value = !this.Value;
    }
}