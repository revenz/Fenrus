using Fenrus.Controllers;
using Fenrus.Models;
using Fenrus.Pages;
using Markdig.Extensions.Tables;
using Renci.SshNet.Messages.Transport;
using Group = Fenrus.Models.Group;

namespace Fenrus.Components;

/// <summary>
/// Page Groups
/// </summary>
public partial class PageGroups : CommonPage<Models.Group>
{
    public List<Models.Group> Items { get; set; } = new();

    private FenrusTable<Group> Table { get; set; }

    private string lblTitle, lblDescription;

    protected override Task PostGotUser()
    {
        lblTitle = Translator.Instant("Pages.Groups.Title" + (IsSystem ? "-System" : string.Empty));
        lblDescription = Translator.Instant("Pages.Groups.Labels.PageDescription" + (IsSystem ? "-System" : string.Empty));
        var service = new GroupService();
        Items = (IsSystem ? service.GetSystemGroups() : service.GetAllForUser(UserUid)).OrderBy(x => x.Name).ToList();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Enables an item
    /// </summary>
    /// <param name="item">the item being enabled</param>
    /// <param name="enabled">the enabled state</param>
    private void ItemEnabled(Models.Group item, bool enabled)
    {
        item.Enabled = enabled;
        new GroupService().Enable(item.Uid, enabled);
    }

    protected override bool DoDelete(Group item)
    {
        var service = new GroupService();
        service.Delete(item.Uid);
        Items = Items.Where(x => x.Uid != item.Uid).ToList();
        Table.SetData(Items);
        return true;
    }
}