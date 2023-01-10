using Fenrus.Components;
using Fenrus.Components.SideEditors;
using Fenrus.Models;
using Fenrus.Services;

namespace Fenrus.Pages;

/// <summary>
/// Users page
/// </summary>
public partial class Users: CommonPage<User>
{
    /// <summary>
    /// Gets or sets the items 
    /// </summary>
    public List<User> Items { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the table instance
    /// </summary>
    private FenrusTable<User> Table { get; set; }

    /// <summary>
    /// Gets the page title
    /// </summary>
    private string Title => "Users";

    /// <summary>
    /// Gets the page description
    /// </summary>
    private string Description =>
        "This page lets you manage users for Fenrus.";
    
    /// <summary>
    /// Called after the user has been fetched
    /// </summary>
    protected override async Task PostGotUser()
    {
        Items = new UserService().GetAll();
    }

    /// <summary>
    /// Add a new docker item
    /// </summary>
    protected async override Task Add()
    {
        // var result = await Popup.OpenEditor<DockerServerEditor, DockerServer>(null);
        // if (result.Success == false)
        //     return;
        // Items.Add(result.Data);
        // Items = Items.OrderBy(x => x.Name).ToList();
        // StateHasChanged();
    }

    /// <summary>
    /// Edit a new docker item
    /// </summary>
    /// <param name="item"></param>
    async Task Edit(User item)
    {
        // var result = await Popup.OpenEditor<DockerServerEditor, DockerServer>(item);
        // if (result.Success == false)
        //     return;
        // item.Address = result.Data.Address;
        // item.Name = result.Data.Name;
        // item.Port = result.Data.Port;
        // Items = Items.OrderBy(x => x.Name).ToList();
        // StateHasChanged();
    }
}