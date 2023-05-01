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
    /// Gets or sets the actual datetime of the start
    /// </summary>
    [JsonIgnore]
    internal DateTime StartDate { get; init; }
    /// <summary>
    /// Gets the end date of the event
    /// </summary>
    [JsonPropertyName("end")]
    public string End { get; init; }
    /// <summary>
    /// Gets or sets the actual datetime of the end
    /// </summary>
    [JsonIgnore]
    internal DateTime EndDate { get; init; }
    
    /// <summary>
    /// Gets if an event is an all day event
    /// </summary>
    public bool AllDay =>  Math.Abs(this.EndDate.Subtract(this.StartDate).TotalDays - 1) < 0.01;

    /// <summary>
    /// Gets if the even is editable
    /// </summary>
    [JsonPropertyName("editable")]
    public bool Editable => !ReadOnly;

    private string? _BackgroundColor;
    
    /// <summary>
    /// Gets or sets the background color for the event
    /// </summary>
    [JsonPropertyName("backgroundColor")]
    public string? BackgroundColor
    {
        get
        {
            if (ReadOnly && string.IsNullOrEmpty(_BackgroundColor))
                return "rgba(var(--danger-rgb), 0.5)";
            return _BackgroundColor?.EmptyAsNull();
        }
        set => _BackgroundColor = value;
    }
    
    /// <summary>
    /// Gets or sets if this event is read-only
    /// </summary>
    [JsonIgnore]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Converts a CalendarEvent to a CalendarEventModel
    /// </summary>
    /// <param name="event">the CalendarEvent</param>
    /// <returns>the CalendarEventModel</returns>
    public static CalendarEventModel From(CalendarEvent @event)
        => From(@event.Uid,
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
            StartDate = start,
            EndDate = end,
            Start = start.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            End = end.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ReadOnly = readOnly
        };
}