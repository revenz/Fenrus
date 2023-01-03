using Fenrus.Models;
using Fenrus.Models.UiModels;
using Fenrus.Services;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.SideEditors;

/// <summary>
/// Group Item Editor
/// </summary>
public partial class GroupItemEditor
{
    [Parameter] public GroupItem Item { get; set; }
    
    [Parameter] public EventCallback<GroupItem> OnSaved { get; set; }
    [Parameter] public EventCallback OnCanceled { get; set; }

    private GroupItemEditorModel Model;
    private string Title;

    private Dictionary<string, FenrusApp> Apps;
    private List<ListOption> SmartApps, BasicApps;

    protected override void OnInitialized()
    {
        Apps = AppService.Apps;
        SmartApps = Apps.Where(x => x.Value.IsSmart == true).OrderBy(x => x.Key)
            .Select(x => new ListOption() { Label = x.Key, Value = x.Key }).ToList();
        
        BasicApps = Apps.Where(x => x.Value.IsSmart == false).OrderBy(x => x.Key)
            .Select(x => new ListOption() { Label = x.Key, Value = x.Key }).ToList();
        
        this.Model = new();
        Title = "Edit Item";
        if (Item is AppItem app)
        {
            Model.ItemType = app.Type;
            Model.Target = app.Target;
            Model.Url = app.Url;
            Model.AppName = app.AppName;
            Model.DockerContainer = app.DockerContainer;
            Model.DockerUid = app.DockerUid;
            Model.DockerCommand = app.DockerCommand;
            Model.SshPassword = app.SshPassword;
            Model.SshServer = app.SshServer;
            Model.SshUserName = app.SshUserName;
            Model.Enabled = app.Enabled;
            Model.Icon = app.Icon;
            Model.Name = app.Name;
            Model.Size = app.Size;
            Model.Uid = app.Uid;
        }
        else if (Item is LinkItem link)
        {
            Model.ItemType = link.Type;
            Model.Target = link.Target;
            Model.Url = link.Url;
            Model.Enabled = link.Enabled;
            Model.Icon = link.Icon;
            Model.Name = link.Name;
            Model.Size = link.Size;
            Model.Uid = link.Uid;
        }
        else
        {
            Title = "New Item";
            Model.ItemType = "DashboardApp";
            Model.Target = string.Empty;
            Model.Url = "https://";
            Model.Enabled = true;
            Model.Icon = string.Empty;
            Model.Name = string.Empty;
            Model.Size = ItemSize.Medium;
            Model.Uid = Guid.Empty;
        }
    }

    async Task Save()
    {
        // validate
        GroupItem result;
        switch (Model.ItemType)
        {
            case "DashboardApp":
                {
                    var app  = new AppItem();
                    app.Target = Model.Target;
                    app.Url = Model.Url;
                    app.AppName = Model.AppName;
                    app.DockerContainer = Model.DockerContainer;
                    app.DockerUid = Model.DockerUid;
                    app.DockerCommand = Model.DockerCommand;
                    app.SshPassword = Model.SshPassword;
                    app.SshServer = Model.SshServer;
                    app.SshUserName = Model.SshUserName;
                    app.Enabled = Model.Enabled;
                    app.Icon = Model.Icon;
                    app.Name = Model.Name;
                    app.Size = Model.Size;
                    app.Uid = Model.Uid;
                    result = app;
                }
                break;
            case "DashboardLink":
                {
                    var link = new LinkItem();
                    link.Target = Model.Target;
                    link.Url = Model.Url;
                    link.Enabled = Model.Enabled;
                    link.Icon = Model.Icon;
                    link.Name = Model.Name;
                    link.Size = Model.Size;
                    link.Uid = Model.Uid;
                    result = link;
                }
                break;
            default:
                throw new Exception("Unknown type: " + Model.ItemType);
        }

        await OnSaved.InvokeAsync(result);
    }

    Task Cancel()
        => OnCanceled.InvokeAsync();
}

class GroupItemEditorModel : GroupItem
{
    public override string Type => ItemType;
    public string ItemType { get; set; }
    public string Url { get; set; }

    public string AppName { get; set; }
    public string Target { get; set; }

    public string SshServer { get; set; }
    public string SshUserName { get; set; }
    public string SshPassword { get; set; }
    public Guid? DockerUid { get; set; }
    public string DockerContainer { get; set; }
    public string DockerCommand { get; set; }
}