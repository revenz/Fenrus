using Fenrus.Models;
using Fenrus.Services.CalendarServices;

namespace Fenrus.Services;

/// <summary>
/// Calendar Feed Service
/// </summary>
public class CalendarFeedService
{
    /// <summary>
    /// Gets the system groups
    /// </summary>
    /// <param name="enabledOnly">If only enabled calendar feeds should be fetched</param>
    /// <returns>a list of all the system calendar feeds</returns>
    public List<CalendarFeed> GetAllSystem(bool enabledOnly = false)
        => DbHelper.GetAll<CalendarFeed>().Where(x => x.IsSystem && x.UserUid == Guid.Empty && (enabledOnly == false || x.Enabled)).ToList();
    
    /// <summary>
    /// Gets all the calendar feeds for all users
    /// </summary>
    /// <returns>a list of calendar feeds</returns>
    public List<CalendarFeed> GetAll()
        => DbHelper.GetAll<CalendarFeed>();
    
    /// <summary>
    /// Gets all the calendar feeds for a user
    /// </summary>
    /// <param name="uid">The users UID</param>
    /// <returns>a list of calendar feeds</returns>
    public List<CalendarFeed> GetAllForUser(Guid uid)
        => DbHelper.GetAllForUser<CalendarFeed>(uid);

    /// <summary>
    /// Gets a calendar feed by its UID
    /// </summary>
    /// <param name="uid">the UID of the object to get</param>
    /// <returns>the calendar feed</returns>
    public CalendarFeed GetByUid(Guid uid)
        => DbHelper.GetByUid<CalendarFeed>(uid);

    /// <summary>
    /// Enables a calendar feed
    /// </summary>
    /// <param name="uid">the UID of the calendar feed to enable</param>
    /// <param name="enabled">the enabled state</param>
    public void Enable(Guid uid, bool enabled)
    {
        var group = GetByUid(uid);
        if (group == null)
            return;
        if (group.Enabled == enabled)
            return; // nothing to do
        group.Enabled = enabled;
        DbHelper.Update(group);
    }

    /// <summary>
    /// Adds a new calendar feed
    /// </summary>
    /// <param name="group">the new calendar feed being added</param>
    public void Add(CalendarFeed group)
    {
        if(group.Uid == Guid.Empty)
            group.Uid = Guid.NewGuid();
        DbHelper.Insert(group);
    }

    /// <summary>
    /// Updates a calendar feed
    /// </summary>
    /// <param name="group">the calendar feed being updated</param>
    public void Update(CalendarFeed group)
        => DbHelper.Update(group);

    /// <summary>
    /// Deletes a calendar feed
    /// </summary>
    /// <param name="uid">the UID of the calendar feed</param>
    public void Delete(Guid uid)
        => DbHelper.Delete<CalendarFeed>(uid);


    /// <summary>
    /// Gets all events from calendar feeds to a given user
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="fromUtc">the from date</param>
    /// <param name="toUtc">the to date</param>
    /// <returns>a list of all events</returns>
    public List<CalendarEventModel> GetEvents(Guid userUid, DateTime fromUtc, DateTime toUtc)
    {
        var feeds = GetAllSystem(true).Union(GetAllForUser(userUid).Where(x => x.Enabled)).ToList();
        List<CalendarEventModel> events = new();
        foreach (var feed in feeds)
        {
            if (feed.Type == CalendarFeedType.iCal)
            {
                if(string.IsNullOrEmpty(feed.Url) == false)
                    events.AddRange(new iCalendarService(feed.Url, feed.CacheMinutes).GetEvents(fromUtc, toUtc));
            }
        }

        return events;
    }
}