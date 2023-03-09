using Fenrus.Models;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.Dialogs;

/// <summary>
/// Copy Item dialog
/// </summary>
public partial class CopyItemDialog: ComponentBase, IDisposable
{
    /// <summary>
    /// Gets or sets the translator to use
    /// </summary>
    [CascadingParameter] public Translator Translator { get; set; }

    /// <summary>
    /// Gets or sets the user settings
    /// </summary>
    [Parameter] public UserSettings Settings { get; set; }
    /// <summary>
    /// Gets or sets if system groups should be shown
    /// </summary>
    [Parameter] public bool SystemGroups { get; set; }
    
    /// <summary>
    /// Gets or sets the UID of the current group
    /// </summary>
    [Parameter] public Guid GroupUid { get; set; }

    private List<Group> SystemGroupList;
    private List<Group> UserGroupList;

    private string lblOk, lblCancel, lblTitle, lblDescription, lblSystem, lblThisGroup, lblUser;
    TaskCompletionSource<Guid?> ShowTask;
    private bool Visible { get; set; }

    private Guid DestinationUid;

    protected override void OnInitialized()
    {
        this.lblOk = Translator.Instant("Labels.Ok");
        this.lblCancel = Translator.Instant("Labels.Cancel");
        this.lblTitle = Translator.Instant("Dialogs.CopyItem.Title");
        this.lblDescription = Translator.Instant("Dialogs.CopyItem.Description");
        this.lblSystem = Translator.Instant("Dialogs.CopyItem.System");
        this.lblUser = Translator.Instant("Dialogs.CopyItem.User");
        this.lblThisGroup = Translator.Instant("Dialogs.CopyItem.ThisGroup");
        // App.Instance.OnEscapePushed += InstanceOnOnEscapePushed;
        SystemGroupList = new GroupService().GetSystemGroups(enabledOnly: false).Where(x => x.Uid != GroupUid).OrderBy(x => x.Name).ToList();
        UserGroupList = Settings?.Groups?.Where(x => x.Uid != GroupUid)?.OrderBy(x => x.Name)?.ToList() ?? new ();
    }

    // private void InstanceOnOnEscapePushed(OnEscapeArgs args)
    // {
    //     if (Visible)
    //     {
    //         No();
    //         this.StateHasChanged();
    //     }
    // }

    public Task<Guid?> Show()
    {
        this.DestinationUid = Guid.Empty;
        this.Visible = true;
        this.StateHasChanged();
        ShowTask = new TaskCompletionSource<Guid?>();
        return ShowTask.Task;
    }

    private async void Ok()
    {
        this.Visible = false;
        ShowTask.TrySetResult(this.DestinationUid);
        await Task.CompletedTask;
    }

    private async void Cancel()
    {
        this.Visible = false;
        ShowTask.TrySetResult(null);
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        // App.Instance.OnEscapePushed -= InstanceOnOnEscapePushed;
    }



    private void SelectionChanged(ChangeEventArgs args)
    {
        if(Guid.TryParse(args.Value?.ToString(), out Guid guid))
            DestinationUid = guid;
    }
     
}