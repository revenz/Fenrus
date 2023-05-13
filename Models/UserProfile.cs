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
    
    #region Email
    /// <summary>
    /// Gets or sets the email IMAP server
    /// </summary>
    public EncryptedString EmailImapServer { get; set; }
    /// <summary>
    /// Gets or sets the email IMAP port
    /// </summary>
    public int EmailImapPort { get; set; }
    /// <summary>
    /// Gets or sets the email IMAP username
    /// </summary>
    public EncryptedString EmailImapUsername { get; set; }
    /// <summary>
    /// Gets or sets the email IMAP password
    /// </summary>
    public EncryptedString EmailImapPassword { get; set; }
    /// <summary>
    /// Gets or sets the email SMTP server
    /// </summary>
    public EncryptedString EmailSmtpServer { get; set; }
    /// <summary>
    /// Gets or sets the email SMTP port
    /// </summary>
    public int EmailSmtpPort { get; set; }
    /// <summary>
    /// Gets or sets the email SMTP username
    /// </summary>
    public EncryptedString EmailSmtpUsername { get; set; }
    /// <summary>
    /// Gets or sets the email SMTP password
    /// </summary>
    public EncryptedString EmailSmtpPassword { get; set; }
    #endregion
}