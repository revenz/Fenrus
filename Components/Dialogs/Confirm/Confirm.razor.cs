using Fenrus.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Fenrus.Components.Dialogs;

/// <summary>
/// Confirm dialog
/// </summary>
public partial class Confirm: ComponentBase, IDisposable
{
    /// <summary>
    /// Gets or sets the javascript runtime to use
    /// </summary>
    [Inject] public IJSRuntime jsRuntime { get; set; }
    
    /// <summary>
    /// Gets or sets the translater to use
    /// </summary>
    [CascadingParameter] public Translater Translater { get; set; }
    
    
    private string lblYes, lblNo, lblConfirm;
    private string Message, Title, SwitchMessage;
    TaskCompletionSource<bool> ShowTask;
    TaskCompletionSource<(bool, bool)> ShowSwitchTask;
    private bool ShowSwitch;
    private bool SwitchState;

    private string btnYesUid; 

    private static Confirm Instance { get; set; }

    private bool Visible { get; set; }
    private bool focused = false;

    protected override void OnInitialized()
    {
        this.lblYes = Translater.Instant("Labels.Yes");
        this.lblNo = Translater.Instant("Labels.No");
        this.lblConfirm = Translater.Instant("Labels.Confirm");
        // App.Instance.OnEscapePushed += InstanceOnOnEscapePushed;
        Instance = this;
    }

    // private void InstanceOnOnEscapePushed(OnEscapeArgs args)
    // {
    //     if (Visible)
    //     {
    //         No();
    //         this.StateHasChanged();
    //     }
    // }

    /// <summary>
    /// Shows a confirm message
    /// </summary>
    /// <param name="title">the title of the confirm message</param>
    /// <param name="message">the message of the confirm message</param>
    /// <returns>the task to await for the confirm result</returns>
    public static Task<bool> Show(string title, string message)
    {
        if (Instance == null)
            return Task.FromResult(false);

        return Instance.ShowInstance(title, message);
    }
    
    /// <summary>
    /// Shows a confirm message
    /// </summary>
    /// <param name="title">the title of the confirm message</param>
    /// <param name="message">the message of the confirm message</param>
    /// <param name="switchMessage">message to show with an extra switch</param>
    /// <param name="switchState">the switch state</param>
    /// <returns>the task to await for the confirm result</returns>
    public static Task<(bool Confirmed, bool SwitchState)> Show(string title, string message, string switchMessage, bool switchState = false)
    {
        if (Instance == null)
            return Task.FromResult((false, false));

        return Instance.ShowInstance(title, message, switchMessage, switchState);
    }

    private Task<bool> ShowInstance(string title, string message)
    {
        //Task.Run(async () =>
        // {
            // wait a short delay this is in case a "Close" from an escape key is in the middle
            // of processing, and if we show this confirm too soon, it may automatically be closed
            // await Task.Delay(5);
            this.btnYesUid = Guid.NewGuid().ToString();
            this.focused = false;
            this.Title = lblConfirm;
            this.Message = message ?? string.Empty;
            this.ShowSwitch = false;
            this.Visible = true;
            this.StateHasChanged();
        //});

        Instance.ShowTask = new TaskCompletionSource<bool>();
        return Instance.ShowTask.Task;
    }
        
    private Task<(bool, bool)> ShowInstance(string title, string message, string switchMessage, bool switchState)
    {
        this.btnYesUid = Guid.NewGuid().ToString();
        this.focused = false;
        this.Title = title?.EmptyAsNull() ?? lblConfirm;
        this.Message = message ?? string.Empty;
        this.SwitchMessage = switchMessage ?? string.Empty;
        this.ShowSwitch = true;
        this.SwitchState = switchState;
        this.Visible = true;
        this.StateHasChanged();

        Instance.ShowSwitchTask  = new TaskCompletionSource<(bool, bool)>();
        return Instance.ShowSwitchTask.Task;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Visible && focused == false)
        {
            focused = true;
            await jsRuntime.InvokeVoidAsync("eval", $"document.getElementById('{this.btnYesUid}').focus()");
        }
    }

    private async void Yes()
    {
        this.Visible = false;
        if (ShowSwitch)
        {
            Instance.ShowSwitchTask.TrySetResult((true, SwitchState));
            await Task.CompletedTask;
        }
        else
        {
            Instance.ShowTask.TrySetResult(true);
            await Task.CompletedTask;
        }
    }

    private async void No()
    {
        this.Visible = false;
        if (ShowSwitch)
        {
            Instance.ShowSwitchTask.TrySetResult((false, SwitchState));
            await Task.CompletedTask;
        }
        else
        {
            Instance.ShowTask.TrySetResult(false);
            await Task.CompletedTask;
        }
    }

    public void Dispose()
    {
        // App.Instance.OnEscapePushed -= InstanceOnOnEscapePushed;
    }
     
}