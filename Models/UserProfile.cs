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
    
    /// <summary>
    /// Gets or sets if users are allowed their cloud/side bar
    /// </summary>
    public CloudFeature CloudFeatures { get; set; }
    
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
    
    #region Apps
    /// <summary>
    /// Gets or sets the cloud app groups for this user
    /// </summary>
    public List<CloudAppGroup> AppGroups { get; set; }
    #endregion
}

/// <summary>
/// A cloud app group
/// </summary>
public class CloudAppGroup
{
    /// <summary>
    /// Gets or sets the name of the cloud app group
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets if this group is enabled
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Gets or sets the apps in the group
    /// </summary>
    public List<CloudApp> Items { get; set; }
}

/// <summary>
/// Cloud app that appears in a users cloud app drawer
/// </summary>
public class CloudApp
{
    /// <summary>
    /// Gets or sets the name of the app
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Gets or sets the Icon for the app
    /// </summary>
    public string Icon { get; set; }
    /// <summary>
    /// Gets or sets the address of the app
    /// </summary>
    public string Address { get; set; }
    /// <summary>
    /// Gets or sets the type of the app
    /// </summary>
    public CloudAppType Type { get; set; }
}

/// <summary>
/// Types of cloud apps
/// </summary>
public enum CloudAppType
{
    /// <summary>
    /// Basic link that opens in the iframe
    /// </summary>
    IFrame = 0,
    /// <summary>
    /// App that opens in this tab
    /// </summary>
    Internal = 1,
    /// <summary>
    /// App that opens in a new tab
    /// </summary>
    External = 2,
    /// <summary>
    /// App that opens in a same tab
    /// </summary>
    ExternalSame = 3,
    /// <summary>
    /// VNC app
    /// </summary>
    Vnc = 4
}