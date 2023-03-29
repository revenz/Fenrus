using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.Dialogs;

/// <summary>
/// Message bog
/// </summary>
public partial class MessageBox: ComponentBase, IDisposable
{
    
    /// <summary>
    /// Gets or sets the translator to use
    /// </summary>
    [CascadingParameter] public Translator Translator { get; set; }
    
    private string lblOk, lblTitle;
    private string Message, Title;
    TaskCompletionSource ShowTask;

    private static MessageBox Instance { get; set; }

    private bool Visible { get; set; }

    protected override void OnInitialized()
    {
        this.lblOk = Translator.Instant("Labels.Ok");
        this.lblTitle = Translator.Instant("Labels.Message");
        Instance = this;
    }

    /// <summary>
    /// Shows a message box
    /// </summary>
    /// <param name="title">the title of the message</param>
    /// <param name="message">the message itself</param>
    /// <returns>the task to await for the message box to close</returns>
    public static Task Show(string title, string message)
    {
        if (Instance == null)
            return Task.FromResult(false);

        return Instance.ShowInstance(title, message);
    }

    private Task ShowInstance(string title, string message)
    {
        this.Title = title?.EmptyAsNull() ?? lblTitle; 
        this.Message = message ?? string.Empty;
        this.Visible = true;
        this.StateHasChanged();

        Instance.ShowTask = new TaskCompletionSource();
        return Instance.ShowTask.Task;
    }

    private async void Close()
    {
        this.Visible = false;
        Instance.ShowTask.SetResult();
        await Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}