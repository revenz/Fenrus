using Fenrus.Models;
using Fenrus.Pages;
using Group = Fenrus.Models.Group;

namespace Fenrus.Components;

/// <summary>
/// Page Groups
/// </summary>
public partial class PageGroups : CommonPage<Models.Group>
{
    public List<Models.Group> Items { get; set; } = new();

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
}