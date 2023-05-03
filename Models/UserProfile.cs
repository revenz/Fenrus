using LiteDB;

namespace Fenrus.Models;

/// <summary>
/// Extended user profile settings
/// </summary>
public class UserProfile:IModalUid
{
    /// <summary>
    /// Get or sets the UID who owns this profile
    /// </summary>
    [BsonId]
    public Guid Uid { get; set; }
    
    #region Calendar
    /// <summary>
    /// Gets or sets the calendar provider
    /// </summary>
    public string CalendarProvider { get; set; }
    /// <summary>
    /// Gets or sets the calendar URL
    /// </summary>
    public EncryptedString CalendarUrl { get; set; }
    /// <summary>
    /// Gets or sets the calendar username
    /// </summary>
    public EncryptedString CalendarUsername { get; set; }
    /// <summary>
    /// Gets or sets the calendar password
    /// </summary>
    public EncryptedString CalendarPassword { get; set; }
    /// <summary>
    /// Gets or sets the calendar name
    /// </summary>
    public EncryptedString CalendarName { get; set; }
    #endregion   
    
    #region File Storage
    /// <summary>
    /// Gets or sets the file storage provider
    /// </summary>
    public string FileStorageProvider { get; set; }
    /// <summary>
    /// Gets or sets the file storage URL
    /// </summary>
    public EncryptedString FileStorageUrl { get; set; }
    /// <summary>
    /// Gets or sets the file storage username
    /// </summary>
    public EncryptedString FileStorageUsername { get; set; }
    /// <summary>
    /// Gets or sets the file storage password
    /// </summary>
    public EncryptedString FileStoragePassword { get; set; }
    #endregion
}