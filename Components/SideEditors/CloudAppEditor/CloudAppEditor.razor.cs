using Blazored.Toast.Services;
using Fenrus.Models;
using Fenrus.Models.UiModels;
using Microsoft.AspNetCore.Components;

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
    /// Gets or sets the address for a SSH app
    /// </summary>
    private string AddressSsh { get; set; }
    
    /// <summary>
    /// Gets or sets the address for a docker app
    /// </summary>
    private string AddressDocker { get; set; }
    /// <summary>
    /// Gets or sets the docker command 
    /// </summary>
    private string DockerCommand { get; set; }

    /// <summary>
    /// Gets or sets the icon of the app
    /// </summary>
    private string Icon { get; set; }

    /// <summary>
    /// Gets or sets the app type
    /// </summary>
    private CloudAppType AppType { get; set; }
    private string SelectedItem;
    /// <summary>
    /// a list of docker servers in the system
    /// </summary>
    private List<ListOption> DockerServers;
    /// <summary>
    /// Gets or sets the UID of the selected docker server
    /// </summary>
    private Guid DockerUid { get; set; }

    private string GetAddress(CloudAppType type)
    {
        if(AppType == CloudAppType.Vnc)
            return AddressVnc;
        if (AppType == CloudAppType.Ssh)
            return AddressSsh;
        if (AppType == CloudAppType.Docker)
            return DockerUid + ":" + AddressDocker + ":" + DockerCommand;
        return Address;
    }
    
    
    private List<ListOption> Types;

    protected override void OnInitialized()
    {
        Title = Translator.Instant("Pages.CloudAppEditor.Title");
        lblSave = Translator.Instant("Labels.Save");
        lblCancel = Translator.Instant("Labels.Cancel");
        
        DockerServers = new DockerService().GetAll()?.OrderBy(x => x.Name)?.Select(x => new ListOption()
        {
            Label = x.Name,
            Value = x.Uid
        })?.ToList() ?? new ();

        Name = Item?.Name ?? string.Empty;
        AppType = Item?.Type ?? CloudAppType.IFrame;
        if (AppType == CloudAppType.Vnc)
            AddressVnc = Item?.Address ?? "http://";
        else if (AppType == CloudAppType.Ssh)
            AddressSsh = Item?.Address ?? string.Empty;
        else if (AppType == CloudAppType.Docker)
        {
            var parts = Item?.Address?.Value?.Split(':');
            if (parts?.Length >= 2 && Guid.TryParse(parts[0], out Guid dockerUid))
            {
                DockerUid = dockerUid;
                AddressDocker = parts[1];
                if (parts.Length > 2)
                    DockerCommand = parts[2];
            }
            else if(DockerServers.Any())
            {
                DockerUid = (Guid)DockerServers.First().Value!;
            }
        }
        else
            Address = Item?.Address ?? "https://";
        Icon = Item?.Icon ?? string.Empty;
        Types = new()
        {
            new() { Label = Translator.Instant($"Enums.{nameof(CloudAppType)}.{nameof(CloudAppType.IFrame)}"), Value = CloudAppType.IFrame },
            new() { Label = Translator.Instant($"Enums.{nameof(CloudAppType)}.{nameof(CloudAppType.Internal)}"), Value = CloudAppType.Internal },
            new() { Label = Translator.Instant($"Enums.{nameof(CloudAppType)}.{nameof(CloudAppType.External)}"), Value = CloudAppType.External },
            new() { Label = Translator.Instant($"Enums.{nameof(CloudAppType)}.{nameof(CloudAppType.ExternalSame)}"), Value = CloudAppType.ExternalSame },
            new() { Label = Translator.Instant($"Enums.{nameof(CloudAppType)}.{nameof(CloudAppType.Vnc)}"), Value = CloudAppType.Vnc },
            new() { Label = Translator.Instant($"Enums.{nameof(CloudAppType)}.{nameof(CloudAppType.Ssh)}"), Value = CloudAppType.Ssh },
        };
        if (DockerServers.Any())
        {
            if(DockerUid == Guid.Empty)
                DockerUid = (Guid)DockerServers.First().Value!;
            DockerCommand = DockerCommand?.EmptyAsNull() ?? "/bin/bash";
            Types.Add(new()
            {
                Label = Translator.Instant($"Enums.{nameof(CloudAppType)}.{nameof(CloudAppType.Docker)}"),
                Value = CloudAppType.Docker
            });
        }
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
        if (AppType == CloudAppType.Docker && DockerUid == Guid.Empty)
            return;
        var item = new CloudApp();
        item.Name = Name;
        item.Address = (EncryptedString)GetAddress(AppType);
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
