using System.Text.Json.Serialization;

namespace Fenrus.Models;


/// <summary>
/// Models for a calender event used by the UI control
/// </summary>
public class CalendarEventModel
{
    /// <summary>
    /// Gets the UID of the event
    /// </summary>
    [JsonPropertyName("id")]
    public string Uid { get; init; }
    /// <summary>
    /// Gets the name of the event
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; init; }
    /// <summary>
    /// Gets the start date of the event
    /// </summary>
    [JsonPropertyName("start")]
    public string Start { get; init; }
    /// <summary>
    /// Gets the end date of the event
    /// </summary>
    [JsonPropertyName("end")]
    public string End { get; init; }
    
    /// <summary>
    /// Gets or sets if this event is read-only
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Converts a CalendarEvent to a CalendarEventModel
    /// </summary>
    /// <param name="event">the CalendarEvent</param>
    /// <returns>the CalendarEventModel</returns>
    public static CalendarEventModel From(CalendarEvent @event)
        => From(@event.Uid.ToString(),
            @event.Name,
            @event.StartUtc,
            @event.EndUtc,
            false);

    /// <summary>
    /// Constructs a new calendar event modal
    /// </summary>
    /// <param name="uid">the UID of the event</param>
    /// <param name="title">the title of the event</param>
    /// <param name="start">when the event starts</param>
    /// <param name="end">when the event ends</param>
    /// <param name="readOnly">if this event is readonly</param>
    /// <returns>the event</returns>
    public static CalendarEventModel From(string uid, string title, DateTime start, DateTime end, bool readOnly)
        => new()
        {
            Uid = uid,
            Title = title, 
            Start = start.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            End = end.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ReadOnly = readOnly
        };
}