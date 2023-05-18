using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.Dialogs;

/// <summary>
/// Icon Picker dialog
/// </summary>
public partial class IconPickerDialog: ComponentBase, IDisposable
{
    
    /// <summary>
    /// Gets or sets the translator to use
    /// </summary>
    [CascadingParameter] public Translator Translator { get; set; }
    
    private string lblOk, lblCancel;
    private string Title;
    private string Value;
    TaskCompletionSource<string> ShowTask;

    private static IconPickerDialog Instance { get; set; }

    private bool Visible { get; set; }

    protected override void OnInitialized()
    {
        this.lblOk = Translator.Instant("Labels.Ok");
        this.lblCancel = Translator.Instant("Labels.Cancel");
        this.Title = Translator.Instant("Labels.IconPicker");
        Instance = this;
    }

    /// <summary>
    /// Open the dialog
    /// </summary>
    /// <returns>the task to await for the icon picker dialog to close</returns>
    public static Task<string> Show()
    {
        if (Instance == null)
            return Task.FromResult(string.Empty);

        return Instance.ShowInstance();
    }

    private Task<string> ShowInstance()
    {
        this.Visible = true;
        this.StateHasChanged();

        Instance.ShowTask = new TaskCompletionSource<string>();
        return Instance.ShowTask.Task;
    }

    private async void Close()
    {
        if (string.IsNullOrEmpty(Value))
            return;
        this.Visible = false;
        Instance.ShowTask.SetResult(Value);
        await Task.CompletedTask;
    }

    private async void Cancel()
    {
        this.Visible = false;
        Instance.ShowTask.SetResult(string.Empty);
        await Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    private void SelectItem(string appName, bool doubleClick = false)
    {
        Value = appName;
        if(doubleClick)
            Close();
    }
}