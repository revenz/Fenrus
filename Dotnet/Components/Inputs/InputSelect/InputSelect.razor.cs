using System.Diagnostics;
using Fenrus.Models.UiModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.JSInterop;

namespace Fenrus.Components.Inputs;

/// <summary>
/// Input Select
/// </summary>
public partial class InputSelect<TItem> : Input<TItem>
{
    private List<ListItem> Items = new();

    private List<ListOption> AllItems = new();

    [Parameter] public bool ShowDescription { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }

    private List<ListOption> _Options;
    [Parameter]
    public List<ListOption> Options
    {
        get => _Options;
        set
        {
            if (_Options == value)
                return;
            _Options = value;
            if(value?.Any() == true)
                Items.AddRange(value);
            InitializeAllItems();
        }
    }

    private void InitializeAllItems()
    {
        AllItems = Items.SelectMany(x =>
        {
            if (x is ListGroup grp)
                return grp.Items;
            return new List<ListOption> { (ListOption)x };
        }).ToList();
    }

    private string Description { get; set; }
    private bool UpdatingValue = false;

    public override bool Focus() => FocusUid();

    private bool _AllowClear = false;

    private string lblSelectOne;

    [Parameter]
    public bool AllowClear
    {
        get => _AllowClear;
        set { _AllowClear = value; }
    }
    
    /// <summary>
    /// Gets or sets if this field is required, only tested if AllowClear is also set
    /// </summary>
    [Parameter] public bool Required { get; set; }

    private int _SelectedIndex = -1;

    /// <summary>
    /// Clears the current value
    /// </summary>
    public void Clear()
    {
        if (AllowClear == false)
            return;
        
        SelectedIndex = -1;
        Value = default;
        jsRuntime.InvokeVoidAsync("eval", $"document.getElementById('{Uid}').options[0].selected = true;");
        StateHasChanged();
    }

    public int SelectedIndex
    {
        get => _SelectedIndex;
        set
        {
            _SelectedIndex = value;
            // if (value == -1 || Options == null || Options.Any() == false)
            //     this.Value = null;
            // else
            //     this.Value = Options.ToArray()[value].Value;
            UpdateDescription();
        }
    }

    private bool ValueMatches(object other)
    {
        if (other == null && Value == null)
            return true;
        if (other == null)
            return false;
        if (Value == null)
            return false;
        string jvalue = JsonSerializer.Serialize(Value);
        string ovalue = JsonSerializer.Serialize(other);
        return jvalue == ovalue;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        InitializeAllItems();
        lblSelectOne = Placeholder?.EmptyAsNull() ?? "Select One";
        ValueUpdated();
    }

    protected override void ValueUpdated()
    {
        if (UpdatingValue)
            return;

        int startIndex = SelectedIndex;
        if (Value != null)
        {
            // var opt = Options.ToArray();
            // var valueJson = JsonSerializer.Serialize(Value);
            // for (int i = 0; i < opt.Length; i++)
            // {
            //     if (opt[i].Value == Value)
            //     {
            //         startIndex = i;
            //         break;
            //     }
            //
            //     string optJson = JsonSerializer.Serialize(opt[i].Value);
            //     if (optJson.ToLower() == valueJson.ToLower())
            //     {
            //         startIndex = i;
            //         break;
            //     }
            //}
        }

        // if (startIndex == -1)
        // {
        //     if (AllowClear)
        //     {
        //         startIndex = -1;
        //     }
        //     else
        //     {
        //         startIndex = 0;
        //         Value = Options.FirstOrDefault()?.Value;
        //     }
        // }
        //
        // if (startIndex != SelectedIndex)
        //     SelectedIndex = startIndex;
    }

    private void SelectionChanged(ChangeEventArgs args)
    {
        try
        {
            if (int.TryParse(args?.Value?.ToString(), out int index))
            {
                SelectedIndex = index;
                if (index >= 0)
                {
                    if(AllItems[index].Value is TItem val)
                        Value = val;
                }
            }

            UpdateDescription();
        }
        finally
        {
            UpdatingValue = false;
        }
    }

    // public override async Task<bool> Validate()
    // {
    //     if (this.SelectedIndex == -1)
    //     {
    //         ErrorMessage = "Required";
    //         return false;
    //     }
    //
    //     return await base.Validate();
    // }

    private void UpdateDescription()
    {
        Description = string.Empty;
        if (this.ShowDescription == false)
            return;

        IDictionary<string, object> dict = Value as IDictionary<string, object>;

        if (dict == null)
        {
            try
            {
                string json = JsonSerializer.Serialize(Value);
                dict = (IDictionary<string, object>)JsonSerializer.Deserialize<System.Dynamic.ExpandoObject>(json);
            }
            catch (Exception)
            {
            }
        }

        if (dict?.ContainsKey("Description") == true)
            Description = dict["Description"]?.ToString() ?? string.Empty;

        this.Help = Description;
        this.StateHasChanged();
    }


    private async Task OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Code == "Enter")
            await OnSubmit.InvokeAsync();
        else if (e.Code == "Escape")
            await OnClose.InvokeAsync();
    }
    
    /// <summary>
    /// Adds a option to the select
    /// </summary>
    /// <param name="option">the option to add</param>
    public void AddOption(ListOption option)
    {
        this.Items.Add(option);
        this.AllItems.Add(option);
        this.StateHasChanged();
    }

    /// <summary>
    /// Adds a group to the select
    /// </summary>
    /// <param name="group">the group to add</param>
    public void AddGroup(ListGroup group)
    {
        this.Items.Add(group);
        if (group.Items?.Any() == true)
        {
            foreach (var item in group.Items)
                this.AllItems.Add(item);
        }

        this.StateHasChanged();
    }

    public override Task<bool> Validate()
    {
        this.ErrorMessage = string.Empty;
        if (AllowClear && Required)
        {
            if (SelectedIndex == -1)
            {
                this.ErrorMessage = "Required";
                return Task.FromResult(false);
            }
        }
        return base.Validate();
    }
}