using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.Inputs;

public partial class InputSelectOption<TItem>
{
    [Parameter]
    public string Label { get; set; }

    private object _Value;

    [Parameter]
    public object Value
    {
        get => _Value;
        set => _Value = value;
    }

    [Parameter]
    public string StringValue
    {
        get => _Value as string;
        set => _Value = value;
    }
    
    [CascadingParameter] InputSelect<TItem> Select { get; set; }
    [CascadingParameter] InputSelectGroup<TItem> Group { get; set; }

    protected override void OnInitialized()
    {
        if (Group != null)
        {
            Group.AddOption(new()
            {
                Label = this.Label,
                Value = this.Value
            });
        }
        else
        {
            Select.AddOption(new()
            {
                Label = this.Label,
                Value = this.Value
            });
        }
    }
}