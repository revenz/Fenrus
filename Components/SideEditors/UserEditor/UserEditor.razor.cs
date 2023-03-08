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

    private string lblSave, lblCancel, lblUsername, lblEmail, lblPassword, lblIsAdmin,
        lblFullName, lblFullNameHelp;

    private const string DUMMY_PASSWORD = "************";
    
    protected override void OnInitialized()
    {
        lblSave = Translator.Instant("Labels.Save");
        lblCancel = Translator.Instant("Labels.Cancel");
        lblFullName = Translator.Instant("Pages.User.Fields.FullName");
        lblFullNameHelp = Translator.Instant("Pages.User.Fields.FullName-Help");
        lblUsername = Translator.Instant("Pages.User.Fields.Username");
        lblEmail = Translator.Instant("Pages.User.Fields.Email");
        lblPassword = Translator.Instant("Pages.User.Fields.Password");
        lblIsAdmin = Translator.Instant("Pages.User.Fields.IsAdmin");
        Model = new();
        IsNew = false;
        
        if (Item != null)
        {
            Title = Translator.Instant("Pages.User.Labels.EditUser");
            Model.Uid = Item.Uid;
            Model.IsAdmin = Item.IsAdmin;
            Model.Username = Item.Username;
            Model.FullName = Item.FullName;
            Model.Password = string.IsNullOrEmpty(Item.Password) ? string.Empty : DUMMY_PASSWORD;
            Model.Email = Item.Email;
        }
        else
        {
            Title = Translator.Instant("Pages.User.Labels.NewUser");
            Model.Uid = Guid.NewGuid();
            Model.Username = string.Empty;
            Model.FullName = string.Empty;
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
            existing.Email = Model.Email;
            existing.Username = Model.Username;
            existing.FullName = Model.FullName;
            if(string.IsNullOrEmpty(Model.Password) == false && Model.Password != DUMMY_PASSWORD && existing.Password != Model.Password)
                existing.Password = BCrypt.Net.BCrypt.HashPassword(Model.Password);
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