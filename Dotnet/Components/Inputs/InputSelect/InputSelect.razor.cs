using Fenrus.Models.UiModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Fenrus.Components.Inputs;

/// <summary>
/// Input Select
/// </summary>
public partial class InputSelect<TItem> : Input<TItem>
{
    private List<ListItem> Items = new();

    [Parameter] public bool ShowDescription { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }

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

    private int _SelectedIndex = -1;

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

    protected override void OnInitialized()
    {
        base.OnInitialized();
        lblSelectOne = "Select One";
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
        if(args.Value is TItem val)
            Value = val;
        try
        {
            if (int.TryParse(args?.Value?.ToString(), out int index))
                SelectedIndex = index;
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
        this.StateHasChanged();
    }

    /// <summary>
    /// Adds a group to the select
    /// </summary>
    /// <param name="group">the group to add</param>
    public void AddGroup(ListGroup group)
    {
        this.Items.Add(group);
        this.StateHasChanged();
    }
}