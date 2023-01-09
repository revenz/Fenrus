using Fenrus.Models;
using Fenrus.Services;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.SideEditors;

/// <summary>
/// Docker Server Editor
/// </summary>
public partial class DockerServerEditor
{
    /// <summary>
    /// Gets or sets the item this is editing, leave null for a new item
    /// </summary>
    [Parameter] public DockerServer? Item { get; set; }
    
    /// <summary>
    /// Gets or sets the callback when this editor is saved
    /// </summary>
    [Parameter] public EventCallback<DockerServer> OnSaved { get; set; }
    
    /// <summary>
    /// Gets or sets the callback when this editor is canceled
    /// </summary>
    [Parameter] public EventCallback OnCanceled { get; set; }

    /// <summary>
    /// Gets or sets the bound model the user is editing
    /// </summary>
    private DockerServer Model;
    
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


    protected override void OnInitialized()
    {
        Model = new();
        IsNew = false;
        if (Item != null)
        {
            Title = "Edit Docker";
            Model.Address = Item.Address;
            Model.Name = Item.Name;
            Model.Uid = Item.Uid;
            Model.Port= Item.Port;
        }
        else
        {
            Title = "New Docker";
            Model.Address = string.Empty;
            Model.Name = string.Empty;
            Model.Uid = Guid.NewGuid();
            Model.Port = 2375;
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
        
        // save it
        new DockerService().Save(this.Model);
        
        await OnSaved.InvokeAsync(Model);
    }


    /// <summary>
    /// Cancels the editor
    /// </summary>
    /// <returns>a task to await</returns>
    Task Cancel()
        => OnCanceled.InvokeAsync();
}