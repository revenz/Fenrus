using Fenrus.Models;

namespace Fenrus.Services.CalendarServices;

/// <summary>
/// Interface for different calendar services in the system
/// </summary>
public interface ICalendarService
{
    /// <summary>
    /// Gets a calendar event by its UID
    /// </summary>
    /// <param name="uid">The UID of the calendar event</param>
    /// <returns>the calendar event</returns>
    Task<CalendarEvent?> GetByUid(string uid);
    
    /// <summary>
    /// Gets all the calendar events for the user
    /// </summary>
    /// <returns>all the calendar events in the system</returns>
    Task<List<CalendarEvent>> GetAll();

    /// <summary>
    /// Saves a calendar event to the database
    /// </summary>
    /// <param name="event">the event to save</param>
    Task<CalendarEvent> Save(CalendarEvent @event);

    /// <summary>
    /// Adds a calendar event to the database
    /// </summary>
    /// <param name="event">the calendar event</param>
    Task<CalendarEvent> Add(CalendarEvent @event);

    /// <summary>
    /// Deletes a calendar event
    /// </summary>
    /// <param name="uid">the UID of the event to delete</param>
    Task Delete(string uid);

    /// <summary>
    /// Gets the calendar service to use for a user
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <returns>the calendar service to use for the user</returns>
    public static ICalendarService GetService(Guid userUid)
    {
        var profile = DbHelper.GetByUid<UserProfile>(userUid);
        switch (profile.CalendarProvider?.ToLowerInvariant() ?? string.Empty)
        {
            case "nextcloud":
                return new CalDavCalendarService(userUid, profile.CalendarProvider!, profile.CalendarUrl,
                    profile.CalendarUsername, profile.CalendarPassword, profile.CalendarName);
            default:
                return new FenrusCalendarService(userUid);
        }
    }
}