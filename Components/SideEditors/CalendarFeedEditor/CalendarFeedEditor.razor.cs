using Fenrus.Models;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.SideEditors;

/// <summary>
/// Calendar Feed Editor
/// </summary>
public partial class CalendarFeedEditor: SideEditorBase
{
    /// <summary>
    /// Gets or sets the item this is editing, leave null for a new item
    /// </summary>
    [Parameter] public CalendarFeed? Item { get; set; }
    
    /// <summary>
    /// Gets or sets the callback when this editor is saved
    /// </summary>
    [Parameter] public EventCallback<CalendarFeed> OnSaved { get; set; }
    
    /// <summary>
    /// Gets or sets the callback when this editor is canceled
    /// </summary>
    [Parameter] public EventCallback OnCanceled { get; set; }
    
    /// <summary>
    /// Gets or sets if this is a system calendar feed
    /// </summary>
    [Parameter] public bool IsSystem { get; set; }
    
    /// <summary>
    /// Gets or sets the user settings
    /// </summary>
    [Parameter] public UserSettings Settings { get; set; }
    
    /// <summary>
    /// Gets or sets the bound model the user is editing
    /// </summary>
    private CalendarFeed Model;
    
    /// <summary>
    /// Gets or sets the title of the editor
    /// </summary>
    private string Title;
    
    /// <summary>
    /// Gets or sets if this is an editor for a new item
    /// </summary>
    private bool IsNew;
    
    /// <summary>
    /// Gets or sets the side editor instance
    /// </summary>
    private SideEditor Editor { get; set; }
    
    private string Icon { get; set; }

    private string lblSave, lblCancel;    
    
    protected override void OnInitialized()
    {
        Model = new();
        IsNew = false;
        this.lblSave = Translator.Instant("Labels.Save");
        this.lblCancel = Translator.Instant("Labels.Cancel");
        
        if (Item != null)
        {
            Title = Translator.Instant("Pages.Calendar.Labels.EditCalendarFeed");
            Model.Name = Item.Name;
            Model.Uid = Item.Uid;
            Model.Url = Item.Url;
            Model.Type = Item.Type;
            Model.Enabled = Item.Enabled;
        }
        else
        {
            Title = Translator.Instant("Pages.Calendar.Labels.NewCalendarFeed");
            Model.Name = string.Empty;
            Model.Uid = Guid.NewGuid();
            Model.Url = string.Empty;
            Model.Type = CalendarFeedType.iCal;
            Model.Enabled = true;
            IsNew = true;
        }
    }

    /// <summary>
    /// Save the editor
    /// </summary>
    async Task Save()
    {
        // validate
        if (await Editor.Validate() == false)
            return;

        if (this.Model.Uid == Guid.Empty)
            this.Model.Uid = Guid.NewGuid();

        string saveImage = ImageHelper.SaveImageFromBase64(Icon);
        
        var service = new CalendarFeedService();
        if (IsNew)
        {
            Model.Uid = Guid.NewGuid();
            Model.IsSystem = IsSystem;
            
            Model.UserUid = IsSystem ? Guid.Empty : Settings.UserUid; 
            service.Add(Model);
        }
        else
        {
            var existing = service.GetByUid(this.Model.Uid);
            existing.Enabled = Model.Enabled;
            existing.IsSystem = IsSystem;

            existing.Name = Model.Name;
            existing.Url = Model.Url;
            service.Update(existing);
        }
        
        await OnSaved.InvokeAsync(Model);
    }


    /// <summary>
    /// Cancels the editor
    /// </summary>
    /// <returns>a task to await</returns>
    Task Cancel()
        => OnCanceled.InvokeAsync();
}