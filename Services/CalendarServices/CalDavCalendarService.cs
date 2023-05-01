using System.Web;
using CalDAV.NET;
using CalDAV.NET.Interfaces;
using CalendarEvent = Fenrus.Models.CalendarEvent;

namespace Fenrus.Services.CalendarServices;

/// <summary>
/// A calendar service connected to a CalDAV provider
/// </summary>
public class CalDavCalendarService:Fenrus.Services.CalendarServices.ICalendarService
{
    /// <summary>
    /// The UID of the user
    /// </summary>
    private Guid UserUid;
    
    /// <summary>
    /// The URL used to connect to
    /// </summary>
    private string Url;

    /// <summary>
    /// The username and password used to connect to the server
    /// </summary>
    private string Username, Password;

    /// <summary>
    /// The name of the calendar
    /// </summary>
    private string Calendar;
    
    /// <summary>
    /// Constructs a CalDAV Calendar Service instance
    /// </summary> 
    /// <param name="userUid">The UID of the user</param>
    /// <param name="provider">the Provider for this CalDAV server</param>
    /// <param name="url">the URL for the CalDAV server</param>
    /// <param name="username">the Username for the CalDAV server</param>
    /// <param name="password">the Password for the CalDAV server</param>
    /// <param name="calendar">the name of the calendar</param>
    public CalDavCalendarService(Guid userUid, string provider, string url, string username, string password, string calendar)
    {
        this.UserUid = userUid;
        this.Username = username;
        this.Password = password;
        this.Calendar = calendar;
        this.Url = GetCalDavUrl(provider, url, username, password, calendar);
    }

    /// <summary>
    /// Gets the client to use to connect ot the CalDAV server
    /// </summary>
    private Client GetClient()
        => new (new Uri(this.Url), this.Username, this.Password);

    /// <summary>
    /// Gets the calendar to use
    /// </summary>
    /// <returns>the calendar to use</returns>
    private async Task<ICalendar?> GetCalendar()
    {
        var client = GetClient();
        var calendar = (await client.GetCalendarsAsync()).FirstOrDefault();
        return calendar;
    }

    /// <summary>
    /// Converts an IEvent to the Fenrus calendar event type
    /// </summary>
    /// <param name="ev">the event to convert</param>
    /// <returns>the Fenrus event type</returns>
    private CalendarEvent Convert(IEvent ev)
    {
        return new CalendarEvent()
        {
            UserUid = this.UserUid,
            Name = ev.Summary,
            Uid = ev.Uid,
            EndUtc = ev.End.ToUniversalTime(),
            StartUtc = ev.Start.ToUniversalTime()
        };
    }

    /// <summary>
    /// Gets a calendar event by its UID
    /// </summary>
    /// <param name="uid">The UID of the calendar event</param>
    /// <returns>the calendar event</returns>
    public async Task<CalendarEvent?> GetByUid(string uid)
    {
        var calendar = await GetCalendar();
        var ev = calendar?.Events?.FirstOrDefault(x => x.Uid == uid);
        if(ev == null)
            return null;
        return Convert(ev);
    }


    /// <summary>
    /// Gets all the calendar events for the user
    /// </summary>
    /// <returns>all the calendar events for the user</returns>
    public async Task<List<CalendarEvent>> GetAll()
    {
        var calendar = await GetCalendar();
        if (calendar == null)
            return new List<CalendarEvent>();
        
        var results = calendar.Events.Select(x => Convert(x)).ToList();
        return results;
    }

    /// <summary>
    /// Saves a calendar event to the database
    /// </summary>
    /// <param name="event">the event to save</param>
    public async Task<CalendarEvent> Save(CalendarEvent @event)
    {
        var calendar = await GetCalendar();
        if (calendar == null)
            throw new Exception("Failed to locate calendar");
        
        IEvent? existing = null;
        if (string.IsNullOrEmpty(@event.Uid) == false)
            existing = calendar.Events.FirstOrDefault(x => x.Uid == @event.Uid);

        if (existing != null)
        {
            existing.Start = @event.StartUtc;
            existing.End = @event.EndUtc;
            existing.Summary = @event.Name;
        }
        else
        {
            var ev = calendar.CreateEvent(@event.Name, @event.StartUtc, @event.EndUtc);
            @event.Uid = ev.Uid;
        }
        await calendar.SaveChangesAsync();
        return (await GetByUid(@event.Uid))!;
    }

    /// <summary>
    /// Adds a calendar event to the database
    /// </summary>
    /// <param name="event">the calendar event</param>
    public async Task<CalendarEvent> Add(CalendarEvent @event)
    {
        var calendar = await GetCalendar();
        if (calendar == null)
            throw new Exception("Failed to locate calendar");
        @event.Uid = calendar.CreateEvent(@event.Name, @event.StartUtc, @event.EndUtc).Uid;
        await calendar.SaveChangesAsync();
        return (await GetByUid(@event.Uid))!;
    }

    /// <summary>
    /// Deletes a calendar event from the system
    /// </summary>
    /// <param name="uid">the UID of the event to delete</param>
    public async Task Delete(string uid)
    {
        var calendar = await GetCalendar();
        if (calendar == null)
            throw new Exception("Failed to locate calendar");
        var ev = calendar.Events.FirstOrDefault(x => x.Uid == uid);
        if (ev == null)
            return;
        calendar.DeleteEvent(ev);
        await calendar.SaveChangesAsync();
    }

    /// <summary>
    /// Tests a CalDAV connection
    /// </summary>
    /// <param name="provider">the provider for the CalDAV</param>
    /// <param name="url">the URL to the CalDAV server</param>
    /// <param name="username">the username for the CalDAV server</param>
    /// <param name="password">the password for the CalDAV server</param>
    /// <param name="calendar">the name of the calendar</param>
    /// <returns></returns>
    public static async Task<(bool Success, string Error)> TestCalDav(string provider, string url, string username, string password, string calendar)
    {
        string calDavUrl = GetCalDavUrl(provider, url, username, password, calendar);
        try
        {
            var client = new Client(new Uri(calDavUrl), username, password);
            var calendars = await client.GetCalendarsAsync();
            if (calendars?.Any() != true)
                return (false, "No calendars found");

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static string GetCalDavUrl(string provider, string url, string username, string password, string calendar)
    {
        if (string.IsNullOrEmpty(provider) || provider == "Custom")
            return url;

        if (provider.ToLowerInvariant() == "nextcloud")
        {
            if (url.Contains(".php"))
                return url;
            if (url.EndsWith("/") == false)
                url += "/";
            return url + "remote.php/dav/calendars/#" + HttpUtility.UrlEncode(username) + "/" + HttpUtility.UrlEncode(calendar?.EmptyAsNull() ?? "Personal");
        }
        
        return url;
    }
}