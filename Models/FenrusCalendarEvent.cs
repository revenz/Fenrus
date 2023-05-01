namespace Fenrus.Models;

/// <summary>
/// An event in the users calendar
/// </summary>
public class FenrusCalendarEvent: IModal, IUserModal
{
    /// <summary>
    /// Gets or sets the Uid of the item
    /// </summary>
    [LiteDB.BsonId]
    public Guid Uid { get; set; }
    
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

    /// <summary>
    /// Converts a FenrusCalendarEvent to a CalendarEvent
    /// </summary>
    /// <param name="value">the FenrusCalendarEvent</param>
    /// <returns>the CalendarEvent</returns>
    public static implicit operator CalendarEvent(FenrusCalendarEvent value)
        => new ()
        {
            UserUid = value.UserUid,
            Name = value.Name,
            Uid = value.Uid.ToString(),
            StartUtc = value.StartUtc,
            EndUtc = value.EndUtc
        };

    /// <summary>
    /// Converts a CalendarEvent to an FenrusCalendarEvent
    /// </summary>
    /// <param name="value">the string value</param>
    /// <returns>the FenrusCalendarEvent</returns>
    public static explicit operator FenrusCalendarEvent(CalendarEvent value)
        => new ()
        {
            Uid = Guid.Parse(value.Uid),
            StartUtc = value.StartUtc,
            EndUtc = value.EndUtc,
            Name = value.Name,
            UserUid = value.UserUid
        };

    /// <summary>
    /// Converts this to a CalendarEvent
    /// </summary>
    /// <returns>the CalendarEvent</returns>
    public CalendarEvent ToCalendarEvent()
        => new ()
        {
            Uid = this.Uid.ToString(),
            StartUtc = this.StartUtc,
            EndUtc = this.EndUtc,
            Name = this.Name,
            UserUid = this.UserUid
        };
}