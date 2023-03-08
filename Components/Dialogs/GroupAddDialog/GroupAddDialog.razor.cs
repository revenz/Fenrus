using Fenrus.Models.UiModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Fenrus.Components.Dialogs;

/// <summary>
/// Group add dialog
/// </summary>
public partial class GroupAddDialog: ComponentBase
{
    [Inject] public IJSRuntime jsRuntime { get; set; }

    /// <summary>
    /// Gets or sets the translater used
    /// </summary>
    [Parameter] public Translater Translater { get; set; }

    TaskCompletionSource<List<ListOption>> ShowTask;

    private readonly List<ListOption> CheckedItems = new();

    private static GroupAddDialog Instance { get; set; }

    private string btnAddUid = Guid.NewGuid().ToString();

    private List<ListOption> Options = new();

    private bool Visible { get; set; }
    private bool focused = false;

    private string lblTitle, lblAdd, lblCancel;

    protected override void OnInitialized()
    {
        // App.Instance.OnEscapePushed += InstanceOnOnEscapePushed;
        Instance = this;
        lblTitle = Translater.Instant("Labels.AddGroup");
        lblAdd = Translater.Instant("Labels.Add");
        lblCancel = Translater.Instant("Labels.Cancel");
    }
    
    /// <summary>
    /// Shows a modal with available list options
    /// </summary>
    /// <param name="options">the options to show</param>
    /// <returns>the task to await for the confirm result</returns>
    public Task<List<ListOption>> Show(List<ListOption> options)
    {
        this.focused = false;
        this.CheckedItems.Clear();
        this.Options = options ?? new();
        this.Visible = true;
        this.StateHasChanged();
        Instance.ShowTask = new ();
        return Instance.ShowTask.Task;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Visible && focused == false)
        {
            focused = true;
            await jsRuntime.InvokeVoidAsync("eval", $"document.getElementById('{this.btnAddUid}').focus()");
        }
    }

    private async void Add()
    {
        this.Visible = false;
        Instance.ShowTask.TrySetResult(CheckedItems);
        await Task.CompletedTask;
    }

    private async void Cancel()
    {
        this.Visible = false;
        Instance.ShowTask.TrySetResult(new ());
        await Task.CompletedTask;
    }
    
    private Task CheckItem(ListOption option, ChangeEventArgs changeEventArgs)
    {
        if (changeEventArgs.Value is bool bValue == false)
            return Task.CompletedTask;
        if (bValue)
        {
            // add to list
            if(CheckedItems.Contains(option) == false)
                CheckedItems.Add(option);
        }
        else
        {
            // remove from list
            if(CheckedItems.Contains(option) == false)
                CheckedItems.Remove(option);
        }
        return Task.CompletedTask;
    }
}