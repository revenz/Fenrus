using Blazored.Toast.Services;
using Fenrus.Models;
using Fenrus.Models.UiModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace Fenrus.Components.SideEditors;

/// <summary>
/// Group Item Editor
/// </summary>
public partial class CloudAppEditor : SideEditorBase
{
    /// <summary>
    /// Gets or sets the toast service
    /// </summary>
    [Inject]
    protected IToastService ToastService { get; set; }

    /// <summary>
    /// Gets or sets the item this is editing, leave null for a new item
    /// </summary>
    [Parameter]
    public CloudApp Item { get; set; }

    /// <summary>
    /// Gets or sets the callback when this editor is saved (and not kept open)
    /// </summary>
    [Parameter]
    public EventCallback<CloudApp> OnSaved { get; set; }

    /// <summary>
    /// Gets or sets the callback when this editor is canceled
    /// </summary>
    [Parameter]
    public EventCallback OnCanceled { get; set; }

    /// <summary>
    /// Gets or sets the side editor instance
    /// </summary>
    private SideEditor Editor { get; set; }

    private string Title, lblCancel, lblSave;
    

    /// <summary>
    /// Gets or sets the name of the item being edited
    /// </summary>
    private string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the address of the app
    /// </summary>
    private string Address { get; set; }
    
    /// <summary>
    /// Gets or sets the address for a VNC app
    /// </summary>
    private string AddressVnc { get; set; }
    
    /// <summary>
    /// Gets or sets the address for a external app
    /// </summary>
    private string AddressExternal { get; set; }


    /// <summary>
    /// Gets or sets the icon of the app
    /// </summary>
    private string Icon { get; set; }
    
    /// <summary>
    /// Gets or sets the app type
    /// </summary>
    private CloudAppType AppType { get; set; }

    private string SelectedItem;
    
    
    private List<ListOption> Types;

    protected override void OnInitialized()
    {
        Title = Translator.Instant("Pages.CloudAppEditor.Title");
        lblSave = Translator.Instant("Labels.Save");
        lblCancel = Translator.Instant("Labels.Cancel");

        Name = Item?.Name ?? string.Empty;
        AppType = Item?.Type ?? CloudAppType.Link;
        if(AppType == CloudAppType.External)
            AddressExternal = Item?.Address ?? string.Empty;
        else if (AppType == CloudAppType.Vnc)
            AddressVnc = Item?.Address ?? "http://";
        else
            Address = Item?.Address ?? "https://";
        Icon = Item?.Icon ?? string.Empty;
        Types = new()
        {
            new() { Label = nameof(CloudAppType.Link), Value = CloudAppType.Link },
            new() { Label = nameof(CloudAppType.External), Value = CloudAppType.External },
            new() { Label = nameof(CloudAppType.Vnc).ToUpper(), Value = CloudAppType.Vnc },
        };
    }

    /// <summary>
    /// Save the editor
    /// </summary>
    /// <exception cref="Exception">throws if the item type being edited is unknown</exception>
    async Task Save()
    {
        // validate
        if (await Editor.Validate() == false)
            return;
        var item = new CloudApp();
        item.Name = Name;
        if(AppType == CloudAppType.Vnc)
            item.Address = AddressVnc;
        else if(AppType == CloudAppType.External)
            item.Address = AddressExternal;
        else
            item.Address = Address;
        item.Icon = Icon;
        item.Type = AppType;
        await OnSaved.InvokeAsync(item);
    }


    /// <summary>
    /// Cancels the editor
    /// </summary>
    /// <returns>a task to await</returns>
    Task Cancel() => OnCanceled.InvokeAsync();
}
