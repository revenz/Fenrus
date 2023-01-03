using Fenrus.Models.UiModels;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.Inputs;

/// <summary>
/// Input select group
/// </summary>
public partial class InputSelectGroup<TItem>
{
    [Parameter]
    public string Label { get; set; }
    
    [Parameter] public RenderFragment ChildContent { get; set; }
    
    [CascadingParameter] InputSelect<TItem> Select { get; set; }

    private List<ListOption>? _Items = new();
    
    [Parameter]
    public List<ListOption> Items
    {
        get => _Items;
        set => _Items = value ?? new();
    }
    
    protected override void OnInitialized()
    {
        Select.AddGroup(new ListGroup()
        {
            Label = this.Label,
            Items = Items
        });
    }
    /// <summary>
    /// Adds a option to the select
    /// </summary>
    /// <param name="option">the option to add</param>
    public void AddOption(ListOption option)
    {
        this._Items.Add(option);
        this.StateHasChanged();
    }
}