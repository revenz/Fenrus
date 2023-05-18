using Fenrus.Models;
using Fenrus.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Fenrus.Components.Dialogs;

/// <summary>
/// Icon Picker dialog
/// </summary>
public partial class IconPickerDialog: ComponentBase
{
    
    /// <summary>
    /// Gets or sets the translator to use
    /// </summary>
    [CascadingParameter] public Translator Translator { get; set; }
    
    private string lblOk, lblCancel;
    private string Title;
    private string Value;
    TaskCompletionSource<string> ShowTask;
    
    /// <summary>
    /// Gets or sets the filter text
    /// </summary>
    private string Filter { get; set; }

    /// <summary>
    /// Gets or sets the singleton instance
    /// </summary>
    private static IconPickerDialog Instance { get; set; }

    /// <summary>
    /// Gets or sets if this is visible
    /// </summary>
    private bool Visible { get; set; }

    private List<AppIcon> Apps;

    /// <summary>
    /// A unique identifier for the filter text input
    /// </summary>
    private readonly string Uid = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets or sets the javascript runtime
    /// </summary>
    [Inject]
    public IJSRuntime JsRuntime { get; set; }

    /// <summary>
    /// If the filter needs to be focused
    /// </summary>
    private bool focusFilter = false;

    /// <summary>
    /// Initializes the component
    /// </summary>
    protected override void OnInitialized()
    {
        Apps = AppService.Apps.Values.Select(x =>
            new AppIcon()
            {
                Name = x.Name,
                Url = $"/apps/{x.Name}/{x.Icon}"
            }
        ).ToList();
        foreach (var file in new DirectoryInfo(Path.Combine(DirectoryHelper.GetWwwRootDirectory(), "images", "icons")).GetFiles("*.svg").OrderBy(x => x.Name.ToLowerInvariant()))
        {
            Apps.Add(new ()
            {
                Name = file.Name.Replace("-", " ").Replace(".svg", string.Empty),
                Url = "/images/icons/" + file.Name
            });
        }

        Apps = Apps.OrderBy(x => x.Name.ToLowerInvariant()).ToList();
        
        this.lblOk = Translator.Instant("Labels.Ok");
        this.lblCancel = Translator.Instant("Labels.Cancel");
        this.Title = Translator.Instant("Labels.IconPicker");
        Instance = this;
    }

    /// <summary>
    /// Called after the the component is rendered
    /// </summary>
    /// <param name="firstRender">if this is the first time the component is being rendered</param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (focusFilter)
        {
            JsRuntime.InvokeVoidAsync("focusElement", Uid);
            focusFilter = false;
        }
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

    /// <summary>
    /// Shows the dialog
    /// </summary>
    /// <returns>the task to await for the dialog to close</returns>
    private Task<string> ShowInstance()
    {
        // clear a value if already set
        Value = string.Empty;
        Filter = string.Empty;
        this.Visible = true;
        focusFilter = true;
        this.StateHasChanged();

        Instance.ShowTask = new TaskCompletionSource<string>();
        return Instance.ShowTask.Task;
    }

    /// <summary>
    /// Closes the dialog as successful
    /// </summary>
    private async void Close()
    {
        if (string.IsNullOrEmpty(Value))
            return;
        this.Visible = false;
        Instance.ShowTask.SetResult(Value);
        await Task.CompletedTask;
    }
    

    /// <summary>
    /// Cancels the dialog and closes it
    /// </summary>
    private async void Cancel()
    {
        this.Visible = false;
        Instance.ShowTask.SetResult(string.Empty);
        await Task.CompletedTask;
    }


    /// <summary>
    /// Selects an item
    /// </summary>
    /// <param name="url">the app that is selected</param>
    /// <param name="doubleClick">if this is a dboule click</param>
    private void SelectItem(string url, bool doubleClick = false)
    {
        Value = url;
        if(doubleClick)
            Close();
    }

    /// <summary>
    /// Handles a keyboard event in the filter
    /// </summary>
    /// <param name="e"></param>
    private void HandleKeyDown(KeyboardEventArgs e)
    {
        // Handle the keydown event here if needed
        // StateHasChanged();
        Value = string.Empty;
    }


    /// <summary>
    /// An application icon
    /// </summary>
    private class AppIcon
    {
        /// <summary>
        /// Gets or sets the name of the icon
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the URL to the icon
        /// </summary>
        public string Url { get; set; }
    }
}