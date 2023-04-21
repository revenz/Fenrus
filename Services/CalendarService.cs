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
    public CalendarEvent GetByUid(Guid uid)
        => DbHelper.GetByUid<CalendarEvent>(uid);
    
    
    /// <summary>
    /// Gets all the calendar events in the system
    /// </summary>
    /// <returns>all the calendar events in the system</returns>
    public List<CalendarEvent> GetAll()
        => DbHelper.GetAll<CalendarEvent>();
    
    /// <summary>
    /// Gets all the calendar events for a user
    /// </summary>
    /// <param name="uid">The users UID</param>
    /// <returns>a list of calendar events</returns>
    public List<CalendarEvent> GetAllForUser(Guid uid)
        => DbHelper.GetAllForUser<CalendarEvent>(uid);

    /// <summary>
    /// Saves a calendar event to the database
    /// </summary>
    /// <param name="event">the event to save</param>
    public void Save(CalendarEvent @event)
    {
        CalendarEvent? existing = null;
        if (@event.Uid != Guid.Empty)
            existing = DbHelper.GetByUid<CalendarEvent>(@event.Uid);

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
    public void Add(CalendarEvent @event)
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
        => DbHelper.Delete<CalendarEvent>(uid);
}