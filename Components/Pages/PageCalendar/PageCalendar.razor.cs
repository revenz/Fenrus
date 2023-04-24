using Fenrus.Components.SideEditors;
using Fenrus.Models;
using Fenrus.Pages;

namespace Fenrus.Components;

/// <summary>
/// Page for Calendars
/// </summary>
public partial class PageCalendar : CommonPage<Models.CalendarFeed>
{
    public List<Models.CalendarFeed> Items { get; set; } = new();

    private FenrusTable<Models.CalendarFeed> Table { get; set; }

    private string lblTitle, lblDescription;

    protected override Task PostGotUser()
    {
        lblTitle = Translator.Instant("Pages.Calendar.Title" + (IsSystem ? "-System" : string.Empty));
        lblDescription = Translator.Instant("Pages.Calendar.Labels.PageDescription" + (IsSystem ? "-System" : string.Empty));
        var service = new CalendarFeedService();
        Items = (IsSystem ? service.GetAllSystem() : service.GetAllForUser(UserUid)).OrderBy(x => x.Name).ToList();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Enables an item
    /// </summary>
    /// <param name="item">the item being enabled</param>
    /// <param name="enabled">the enabled state</param>
    private void ItemEnabled(CalendarFeed item, bool enabled)
    {
        item.Enabled = enabled;
        new CalendarFeedService().Enable(item.Uid, enabled);
    }

    protected override bool DoDelete(CalendarFeed item)
    {
        var service = new CalendarFeedService();
        service.Delete(item.Uid);
        Items = Items.Where(x => x.Uid != item.Uid).ToList();
        Table.SetData(Items);
        return true;
    }
    /// <summary>
    /// Edits a calendar feed
    /// </summary>
    /// <param name="feed">the calendar feed being edited</param>
    private async Task Edit(CalendarFeed feed)
    {
        var result = await Popup.OpenEditor<CalendarFeedEditor, CalendarFeed>(Translator, feed, new ()
        {
            { nameof(CalendarFeedEditor.IsSystem), IsSystem },
            { nameof(CalendarFeedEditor.Settings), Settings }
        });
        if (result.Success == false)
            return;
        feed.Name = result.Data.Name;
        feed.Url = result.Data.Url;
        feed.Type = result.Data.Type;
        feed.Color = result.Data.Color;
        feed.CacheMinutes = result.Data.CacheMinutes;
        Items = Items.OrderBy(x => x.Name).ToList();
        Table.SetData(Items);
    }
    
    /// <summary>
    /// Adds a new calendar feed
    /// </summary>
    protected override async Task Add()
    {
        var result = await Popup.OpenEditor<CalendarFeedEditor, CalendarFeed>(Translator, null, new ()
        {
            { nameof(CalendarFeedEditor.IsSystem), IsSystem },
            { nameof(CalendarFeedEditor.Settings), Settings }
        });
        if (result.Success == false)
            return;
        Items.Add(result.Data);
        Items = Items.OrderBy(x => x.Name).ToList();
        Table.SetData(Items);
    }
}