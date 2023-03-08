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

    private string lblTitle, lblDescription;
    
    /// <summary>
    /// Called after the user has been fetched
    /// </summary>
    protected override async Task PostGotUser()
    {
        lblTitle = Translator.Instant("Pages.Docker.Title");
        lblDescription = Translator.Instant("Pages.Docker.Labels.PageDescription");
        Items = new DockerService().GetAll();
    }

    /// <summary>
    /// Add a new docker item
    /// </summary>
    protected async override Task Add()
    {
        var result = await Popup.OpenEditor<DockerServerEditor, DockerServer>(Translator, null);
        if (result.Success == false)
            return;
        Items.Add(result.Data);
        Items = Items.OrderBy(x => x.Name).ToList();
        Table.SetData(Items);
    }

    /// <summary>
    /// Edit a new docker item
    /// </summary>
    /// <param name="item"></param>
    async Task Edit(DockerServer item)
    {
        var result = await Popup.OpenEditor<DockerServerEditor, DockerServer>(Translator, item);
        if (result.Success == false)
            return;
        item.Address = result.Data.Address;
        item.Name = result.Data.Name;
        item.Port = result.Data.Port;
        Items = Items.OrderBy(x => x.Name).ToList();
        Table.SetData(Items);
    }

    /// <summary>
    /// Actually performs the deletion after confirmation has been received
    /// </summary>
    /// <param name="item">the item being deleted</param>
    /// <returns>if the deletion was successful</returns>
    protected override bool DoDelete(DockerServer item)
    {
        if (base.DoDelete(item) == false)
            return false;
        
        Items.RemoveAll(x => x.Uid == item.Uid);
        Table.SetData(Items);
        return true;
    }
}