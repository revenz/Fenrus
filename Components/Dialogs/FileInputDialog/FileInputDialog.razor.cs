using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Fenrus.Components.Dialogs;

public partial class FileInputDialog
{
    private static FileInputDialog Instance { get; set; }
    private bool Visible { get; set; }

    TaskCompletionSource<string> ShowTask;
    private string Accept;

    private readonly string Uid = Guid.NewGuid().ToString();

    private string? Filename;
    private IBrowserFile? browserFile;
    
    /// <summary>
    /// Gets or sets the translator used
    /// </summary>
    [Parameter] public Translator Translator { get; set; }

    private string lblTitle, lblOk, lblCancel, lblDescription, lblBrowse;

    protected override void OnInitialized()
    {
        // App.Instance.OnEscapePushed += InstanceOnOnEscapePushed;
        Instance = this;
        lblCancel = Translator.Instant("Labels.Cancel");
        lblBrowse = Translator.Instant("Labels.Browse");
    }
    /// <summary>
    /// Shows a modal window
    /// </summary>
    /// <param name="title">The title of the modal window</param>
    /// <param name="description">The description of the modal window</param>
    /// <param name="okButton">The ok button of the modal window</param>
    /// <param name="accept">The file types to allow eg csv,xlsx</param>
    /// <returns>the task to await for the confirm result</returns>
    public Task<string> Show(string title, string description, string okButton, string accept)
    {
        this.lblTitle = title;
        this.lblDescription = description;
        this.lblOk = okButton;
        this.Accept = accept;
        this.Visible = true;
        this.StateHasChanged();
        Instance.ShowTask = new ();
        return Instance.ShowTask.Task;
    }
    
    private async void OkAction()
    {
        if (this.browserFile == null)
            return;
        
        // maximum of 50MiB
        var content = await new StreamReader(this.browserFile.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024)).ReadToEndAsync();
        
        this.Visible = false;
        Instance.ShowTask.TrySetResult(content);
        await Task.CompletedTask;
        
    }

    private async void Cancel()
    {
        
        this.Visible = false;
        Instance.ShowTask.TrySetResult(string.Empty);
        await Task.CompletedTask;
    }

    private async Task LoadFiles(InputFileChangeEventArgs e)
    {
        if (e.FileCount == 0)
        {
            Filename = string.Empty;
            this.browserFile = null;
            return;
        }

        Filename = e.File.Name;
        this.browserFile = e.File;
    }
}