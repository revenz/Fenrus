using Fenrus.Components;
using Fenrus.Components.SideEditors;
using Fenrus.Models;
using Fenrus.Services;

namespace Fenrus.Pages;

/// <summary>
/// Page for Docker servers
/// </summary>
public partial class Docker: CommonPage<Models.DockerServer>
{
    /// <summary>
    /// Gets or sets the items 
    /// </summary>
    public List<Models.DockerServer> Items { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the table instance
    /// </summary>
    private FenrusTable<Models.DockerServer> Table { get; set; }

    /// <summary>
    /// Gets the page title
    /// </summary>
    private string Title => "Docker";

    /// <summary>
    /// Gets the page description
    /// </summary>
    private string Description =>
        "This page lets you configure Docker instances which can be used in Apps and Links to open terminals into.";
    
    /// <summary>
    /// Called after the user has been fetched
    /// </summary>
    protected override async Task PostGotUser()
    {
        Items = new DockerService().GetAll();
    }

    /// <summary>
    /// Add a new docker item
    /// </summary>
    protected async override Task Add()
    {
        var result = await Popup.OpenEditor<DockerServerEditor, DockerServer>(null);
        if (result.Success == false)
            return;
        Items.Add(result.Data);
        Items = Items.OrderBy(x => x.Name).ToList();
        StateHasChanged();
    }

    /// <summary>
    /// Edit a new docker item
    /// </summary>
    /// <param name="item"></param>
    async Task Edit(DockerServer item)
    {
        var result = await Popup.OpenEditor<DockerServerEditor, DockerServer>(item);
        if (result.Success == false)
            return;
        item.Address = result.Data.Address;
        item.Name = result.Data.Name;
        item.Port = result.Data.Port;
        Items = Items.OrderBy(x => x.Name).ToList();
        StateHasChanged();
    }
}