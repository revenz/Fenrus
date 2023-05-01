using Fenrus.Models;

namespace Fenrus.Services.CalendarServices;

/// <summary>
/// A locally stored Calendar Service
/// </summary>
public class FenrusCalendarService:ICalendarService
{
    /// <summary>
    /// The UID of the user
    /// </summary>
    private Guid UserUid;
    
    /// <summary>
    /// Constructs a Fenrus Calendar Service instance
    /// </summary>
    /// <param name="userUid">The UID of the user</param>
    public FenrusCalendarService(Guid userUid)
    {
        this.UserUid = userUid;
    }

    /// <summary>
    /// Gets a calendar event by its UID
    /// </summary>
    /// <param name="uid">The UID of the calendar event</param>
    /// <returns>the calendar event</returns>
    public Task<CalendarEvent?> GetByUid(string uid)
    {
        var ev = DbHelper.GetByUid<FenrusCalendarEvent>(Guid.Parse(uid));
        return Task.FromResult(ev == null ? null : (CalendarEvent)ev);
    }
    
    
    /// <summary>
    /// Gets all the calendar events for the user
    /// </summary>
    /// <returns>all the calendar events for the user</returns>
    public Task<List<CalendarEvent>> GetAll()
        => Task.FromResult(DbHelper.GetAllForUser<FenrusCalendarEvent>(this.UserUid).Cast<CalendarEvent>().ToList());
    
    /// <summary>
    /// Saves a calendar event to the database
    /// </summary>
    /// <param name="ev">the event to save</param>
    public Task<CalendarEvent> Save(CalendarEvent ev)
    {
        var @event = (FenrusCalendarEvent)ev;
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

        return Task.FromResult((CalendarEvent)@event);
    }

    /// <summary>
    /// Adds a calendar event to the database
    /// </summary>
    /// <param name="ev">the calendar event</param>
    public Task<CalendarEvent> Add(CalendarEvent ev)
    {
        var @event = (FenrusCalendarEvent)ev; 
        if(@event.Uid == Guid.Empty)
            @event.Uid = Guid.NewGuid();
        DbHelper.Insert(@event);
        return Task.FromResult((CalendarEvent)@event);
    }

    /// <summary>
    /// Deletes a calendar event from the system
    /// </summary>
    /// <param name="uid">the UID of the event to delete</param>
    public Task Delete(string uid)
    {
        DbHelper.Delete<FenrusCalendarEvent>(Guid.Parse(uid));
        return Task.CompletedTask;
    }
}