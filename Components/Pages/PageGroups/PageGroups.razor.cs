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

    protected override async Task PostGotUser()
    {
        lblTitle = Translater.Instant("Pages.Groups.Title" + (IsSystem ? "-System" : string.Empty));
        lblDescription = Translater.Instant("Pages.Groups.Labels.PageDescription" + (IsSystem ? "-System" : string.Empty));
        if (IsSystem)
            Items = DbHelper.GetAll<Models.Group>();
        else
            Items = Settings.Groups;
    }

    /// <summary>
    /// Enables an item
    /// </summary>
    /// <param name="item">the item being enabled</param>
    /// <param name="enabled">the enabled state</param>
    private void ItemEnabled(Models.Group item, bool enabled)
    {
        item.Enabled = enabled;
        if (IsSystem)
            new Services.GroupService().Enable(item.Uid, enabled);
        else
            Settings.Save();
    }

    protected override bool DoDelete(Group item)
    {
        if (IsSystem)
        {
            new Services.GroupService().Delete(item.Uid);
        }
        else
        {
            Settings.Groups = Settings.Groups.Where(x => x.Uid != item.Uid).ToList();
            Settings.Save();
        }

        Items = Items.Where(x => x.Uid != item.Uid).ToList();
        Table.SetData(Items);
        return true;
    }
}