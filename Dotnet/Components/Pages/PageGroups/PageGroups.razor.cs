using Fenrus.Models;
using Fenrus.Pages;

namespace Fenrus.Components;

/// <summary>
/// Page Groups
/// </summary>
public partial class PageGroups : CommonPage<Models.Group>
{
    public List<Models.Group> Items { get; set; } = new();

    private string Title => IsSystem ? "System Groups" : "Groups";

    private string Description => IsSystem
        ? @"This page lets you configure groups that will be available to all users and will be available to use on the Guest dashboard.

If you disable a group here that group will become unavailable to all users using it."
        : @"This page lets you create groups which can be used on Dashboards.
                                                                                     
A group will not appear by itself, it must be added to a dashboard.";

    protected override async Task PostGotUser()
    {
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