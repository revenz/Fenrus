using System.Web;
using CalDAV.NET;
using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Service used to manage calendar servers
/// </summary>
public class CalendarService
{
    /// <summary>
    /// Gets a calendar event by its UID
    /// </summary>
    /// <param name="uid">The UID of the calendar event</param>
    /// <returns>the calendar event</returns>
    public FenrusCalendarEvent GetByUid(Guid uid)
        => DbHelper.GetByUid<FenrusCalendarEvent>(uid);
    
    
    /// <summary>
    /// Gets all the calendar events in the system
    /// </summary>
    /// <returns>all the calendar events in the system</returns>
    public List<FenrusCalendarEvent> GetAll()
        => DbHelper.GetAll<FenrusCalendarEvent>();
    
    /// <summary>
    /// Gets all the calendar events for a user
    /// </summary>
    /// <param name="uid">The users UID</param>
    /// <returns>a list of calendar events</returns>
    public List<FenrusCalendarEvent> GetAllForUser(Guid uid)
        => DbHelper.GetAllForUser<FenrusCalendarEvent>(uid);

    /// <summary>
    /// Saves a calendar event to the database
    /// </summary>
    /// <param name="event">the event to save</param>
    public void Save(FenrusCalendarEvent @event)
    {
        FenrusCalendarEvent? existing = null;
        if (@event.Uid != Guid.Empty)
            existing = DbHelper.GetByUid<FenrusCalendarEvent>(@event.Uid);

        if (existing != null)
        {
            DbHelper.Update(@event);
        }
        else
        {
            if (@event.Uid == Guid.Empty)
                @event.Uid = Guid.NewGuid();
            DbHelper.Insert(@event);
        }
    }

    /// <summary>
    /// Adds a calendar event to the database
    /// </summary>
    /// <param name="event">the calendar event</param>
    public void Add(FenrusCalendarEvent @event)
    {
        if(@event.Uid == Guid.Empty)
            @event.Uid = Guid.NewGuid();
        DbHelper.Insert(@event);
    }

    /// <summary>
    /// Deletes a calendar event from the system
    /// </summary>
    /// <param name="uid">the UID of the event to delete</param>
    public void Delete(Guid uid)
        => DbHelper.Delete<FenrusCalendarEvent>(uid);


}