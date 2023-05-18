using Ical.Net;
using Ical.Net.CalendarComponents;
using System.Runtime.Caching;

namespace Fenrus.Services.CalendarServices;

/// <summary>
/// A class that reads iCal feeds and returns events within a specified date range.
/// </summary>
public class iCalService
{
    private readonly string _url;
    private readonly MemoryCache _cache;
    private readonly int _cacheMinutes;

    /// <summary>
    /// Initializes a new instance of the iCalendarService class with the specified iCal feed URL.
    /// </summary>
    /// <param name="url">The URL of the iCal feed to read.</param>
    /// <param name="cacheMinutes">the number of minutes to cache the data for</param>
    public iCalService(string url, int cacheMinutes)
    {
        _url = url;
        _cache = MemoryCache.Default;
        _cacheMinutes = cacheMinutes < 1 ? 15 : cacheMinutes;
    }

    /// <summary>
    /// Returns a list of iCal events between the specified start and end dates.
    /// </summary>
    /// <param name="startDate">The start date for the range of events to return.</param>
    /// <param name="endDate">The end date for the range of events to return.</param>
    /// <returns>A list of iCal events between the specified start and end dates.</returns>
    public List<Models.CalendarEventModel> GetEvents(DateTime startDate, DateTime endDate)
    {
        // Try to get the iCal source from cache
        string cacheKey = _url;
        string? icalSource = _cache.Get(cacheKey) as string;

        // If the iCal source is not in cache, download and cache it
        if (icalSource == null)
        {
            icalSource = DownloadICalSource();
            if (icalSource != null)
            {
                CacheItemPolicy cachePolicy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(_cacheMinutes)
                };
                _cache.Set(cacheKey, icalSource, cachePolicy);
            }
        }

        if (string.IsNullOrWhiteSpace(icalSource))
            return new();

        // Parse the iCal source into a Calendar object
        Calendar calendar = Calendar.Load(icalSource);

        // Get the events between the start and end dates
        List<Models.CalendarEventModel> events = new ();
        foreach (CalendarEvent calendarEvent in calendar.Events)
        {
            if (calendarEvent.Start.Date >= startDate.Date && calendarEvent.Start.Date <= endDate.Date)
            {
                events.Add(Models.CalendarEventModel.From(calendarEvent.Uid, calendarEvent.Summary?.EmptyAsNull() ?? calendarEvent.Name,
                    calendarEvent.Start.AsUtc, calendarEvent.End.AsUtc, true));
            }
        }
        return events;
    }

    private string? DownloadICalSource()
    {
        try
        {
            
            HttpResponseMessage response = Globals.Client.GetAsync(_url).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
