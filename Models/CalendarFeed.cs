namespace Fenrus.Models;

/// <summary>
/// Calendar feed
/// </summary>
public class CalendarFeed: IModal, IUserModal
{
    /// <summary>
    /// Gets or sets the Uid of this
    /// </summary>
    [LiteDB.BsonId]
    public Guid Uid { get; set; }

    /// <summary>
    /// Gets or sets the users UID
    /// </summary>
    public Guid UserUid { get; set; }

    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets if this is enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets if this is a system item
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Gets or sets the URL of the feed
    /// </summary>
    public EncryptedString Url { get; set; }
    
    /// <summary>
    /// Gets or sets the type of feed
    /// </summary>
    public CalendarFeedType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the number of minutes to cache the data 
    /// </summary>
    public int CacheMinutes { get; set; }
}

/// <summary>
/// Types of calendar feeds
/// </summary>
public enum CalendarFeedType
{
    /// <summary>
    /// An iCal feed
    /// </summary>
    iCal = 1
}