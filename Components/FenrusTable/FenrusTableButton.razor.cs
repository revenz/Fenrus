using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Fenrus Table Button
/// </summary>
public partial class FenrusTableButton : ComponentBase
{
    [CascadingParameter] FenrusTableBase Table { get; set; }

    protected string _Label = string.Empty;
    [Parameter]
    public string Label
    {
        get => _Label;
        set => _Label = value;
    }

    protected string _Icon;

    [Parameter]
    public string Icon
    {
        get => _Icon;
        set { _Icon = value ?? string.Empty; }
    }

    [Parameter]
    public bool Disabled { get; set; }

    private bool _Enabled = true;
    public bool Enabled
    {
        get
        {
            if (Disabled) return false;
            return _Enabled;
        }
    }

    [Parameter]
    public EventCallback Clicked { get; set; }

    [Parameter]
    public bool SelectedOne { get; set; }
    [Parameter]
    public bool SelectedOneOrMore { get; set; }

    public virtual async Task OnClick()
    {
        await this.Clicked.InvokeAsync();
    }
    protected override void OnInitialized()
    {
        if (this.Table != null)
        {
            this.Table.AddButton(this);
            this.Table.SelectionChanged += Table_SelectionChanged;
        }

        Table_SelectionChanged(null);
    }


    private void Table_SelectionChanged(List<object> items)
    {
        bool current = this.Enabled;
        var count = items?.Count ?? 0;
        if (this.SelectedOne)
            this._Enabled = count == 1;
        else if (this.SelectedOneOrMore)
            this._Enabled = count > 0;
        else
            this._Enabled = true;
        if (current != this.Enabled)
            this.StateHasChanged();
    }

    public void Dispose()
    {
        if (this.Table != null)
            this.Table.SelectionChanged -= Table_SelectionChanged;
    }
}