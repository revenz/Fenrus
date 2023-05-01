namespace Fenrus.Models;

/// <summary>
/// An event in the users calendar
/// </summary>
public class CalendarEvent
{
    /// <summary>
    /// Gets or sets the Uid of the item
    /// </summary>
    public string Uid { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the event
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the users UID
    /// </summary>
    public Guid UserUid { get; set; }
    
    /// <summary>
    /// Gets or sets the start date of this event
    /// </summary>
    public DateTime StartUtc { get; set; }
    
    /// <summary>
    /// Gets or sets the end date of this event
    /// </summary>
    public DateTime EndUtc { get; set; }
}