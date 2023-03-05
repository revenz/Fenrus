using Fenrus.Models;
using Fenrus.Services;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.SideEditors;

/// <summary>
/// Search Engine Editor
/// </summary>
public partial class UserEditor: SideEditorBase
{
    /// <summary>
    /// Gets or sets the item this is editing, leave null for a new item
    /// </summary>
    [Parameter] public User? Item { get; set; }
    
    /// <summary>
    /// Gets or sets the callback when this editor is saved
    /// </summary>
    [Parameter] public EventCallback<User> OnSaved { get; set; }
    
    /// <summary>
    /// Gets or sets the callback when this editor is canceled
    /// </summary>
    [Parameter] public EventCallback OnCanceled { get; set; }
    
    /// <summary>
    /// Gets or sets the user settings
    /// </summary>
    [Parameter] public UserSettings Settings { get; set; }
    
    /// <summary>
    /// Gets or sets the bound model the user is editing
    /// </summary>
    private User Model;
    
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

    private string lblSave, lblCancel, lblName, lblUsername, lblEmail, lblPassword, lblIsAdmin;
    
    protected override void OnInitialized()
    {
        lblSave = Translater.Instant("Labels.Save");
        lblCancel = Translater.Instant("Labels.Cancel");
        lblName = Translater.Instant("Pages.User.Fields.Name");
        lblUsername = Translater.Instant("Pages.User.Fields.Username");
        lblEmail = Translater.Instant("Pages.User.Fields.Email");
        lblPassword = Translater.Instant("Pages.User.Fields.Password");
        lblIsAdmin = Translater.Instant("Pages.User.Fields.IsAdmin");
        Model = new();
        IsNew = false;
        
        if (Item != null)
        {
            Title = Translater.Instant("Pages.User.Labels.EditUser");
            Model.Name = Item.Name;
            Model.Uid = Item.Uid;
            Model.IsAdmin = Item.IsAdmin;
            Model.Username = Item.Username;
        }
        else
        {
            Title = Translater.Instant("Pages.User.Labels.NewUser");
            Model.Name = string.Empty;
            Model.Uid = Guid.NewGuid();
            Model.Username = string.Empty;
            Model.IsAdmin = false;
            Model.Password = string.Empty;
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

        var service = new UserService();
        if (IsNew)
        {
            Model.Uid = Guid.NewGuid();
            Model.Password = BCrypt.Net.BCrypt.HashPassword(Model.Password);
            service.Add(Model);
        }
        else
        {
            var existing = service.GetByUid(Model.Uid);
            existing.IsAdmin = Model.IsAdmin;
            existing.Name = Model.Name;
            if(existing.Password != Model.Password)
                existing.Password = BCrypt.Net.BCrypt.HashPassword(Model.Password);
            existing.Username = Model.Username;
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