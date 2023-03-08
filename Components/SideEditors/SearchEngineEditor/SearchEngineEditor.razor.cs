using Fenrus.Models;
using Fenrus.Services;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.SideEditors;

/// <summary>
/// Search Engine Editor
/// </summary>
public partial class SearchEngineEditor: SideEditorBase
{
    /// <summary>
    /// Gets or sets the item this is editing, leave null for a new item
    /// </summary>
    [Parameter] public SearchEngine? Item { get; set; }
    
    /// <summary>
    /// Gets or sets the callback when this editor is saved
    /// </summary>
    [Parameter] public EventCallback<SearchEngine> OnSaved { get; set; }
    
    /// <summary>
    /// Gets or sets the callback when this editor is canceled
    /// </summary>
    [Parameter] public EventCallback OnCanceled { get; set; }
    
    /// <summary>
    /// Gets or sets if this is a system search engine
    /// </summary>
    [Parameter] public bool IsSystem { get; set; }
    
    /// <summary>
    /// Gets or sets the user settings
    /// </summary>
    [Parameter] public UserSettings Settings { get; set; }
    
    /// <summary>
    /// Gets or sets the bound model the user is editing
    /// </summary>
    private SearchEngine Model;
    
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

    private string InitialImage;    
    
    protected override void OnInitialized()
    {
        Model = new();
        IsNew = false;
        
        if (Item != null)
        {
            Title = Translator.Instant("Pages.SearchEngines.Labels.EditSearchEngine");
            Model.Name = Item.Name;
            Model.Uid = Item.Uid;
            Model.Shortcut = Item.Shortcut;
            Model.Url = Item.Url;
            Model.Enabled = Item.Enabled;
            if (string.IsNullOrEmpty(Item.Icon) == false)
            {
                if (Item.Icon.StartsWith("db:/image/"))
                {
                    // db image
                    InitialImage = "/fimage/" + Item.Icon["db:/image/".Length..];
                }
                else
                {
                    InitialImage = Item.Icon;
                }
            }
        }
        else
        {
            Title = Translator.Instant("Pages.SearchEngines.Labels.NewSearchEngine");
            Model.Name = string.Empty;
            Model.Uid = Guid.NewGuid();
            Model.Url = string.Empty;
            Model.Shortcut = string.Empty;
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
        
        if (IsNew)
        {
            Model.Icon = saveImage;
            Model.Uid = Guid.NewGuid();
            Model.IsSystem = IsSystem;
            
            if (IsSystem)
                new SearchEngineService().Add(Model);
            else
            {
                Settings.SearchEngines.Add(Model);
                Settings.Save();
            }
        }
        else
        {
            var existing = GetSearchEngine();
            existing.Enabled = Model.Enabled;
            existing.IsDefault = Model.IsDefault;
            existing.IsSystem = IsSystem;
            if (string.IsNullOrEmpty(saveImage) == false)
            {
                // delete existing icon
                ImageHelper.DeleteImage(existing.Icon);
                // now set the new one
                existing.Icon = saveImage;
            }

            existing.Name = Model.Name;
            existing.Shortcut = Model.Shortcut;
            existing.Url = Model.Url;
            if (IsSystem)
                new SearchEngineService().Update(existing);
            else
                Settings.Save();
        }
        
        await OnSaved.InvokeAsync(Model);
    }

    /// <summary>
    /// Gets the search engine
    /// </summary>
    /// <returns>the search engine</returns>
    private SearchEngine? GetSearchEngine()
    {
        if (IsSystem)
            return new SearchEngineService().GetByUid(this.Model.Uid);
        return Settings.SearchEngines.FirstOrDefault(x => x.Uid == Model.Uid);
    }

    /// <summary>
    /// Cancels the editor
    /// </summary>
    /// <returns>a task to await</returns>
    Task Cancel()
        => OnCanceled.InvokeAsync();
}