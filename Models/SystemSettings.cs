using Fenrus.Controllers;
using Fenrus.Services;
using LiteDB;

namespace Fenrus.Models;

/// <summary>
/// Global system settings
/// </summary>
public class SystemSettings
{
    // hardcoded UID for settings
    private readonly Guid _Uid = new Guid("e0bbac69-86bf-4b76-b8a6-3e28bf92d37f");
    
    /// <summary>
    /// Gets or sets the UID for settings
    /// </summary>
    [BsonId]
    public Guid Uid
    {
        get => _Uid;
        set { }
    }


    /// <summary>
    /// Gets or sets if registrations are allowed
    /// </summary>
    public bool AllowRegister { get; set; }
    
    /// <summary>
    /// Gets or sets if guests are allowed
    /// </summary>
    public bool AllowGuest { get; set; }
    
    /// <summary>
    /// Gets or sets the system language
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    /// Gets the authentication strategy
    /// </summary>
    public AuthStrategy Strategy { get; set; }

    /// <summary>
    /// Gets or sets the OAuth strategy issuer base url
    /// </summary>
    public string OAuthStrategyIssuerBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the OAuth strategy client id
    /// </summary>
    public string OAuthStrategyClientId { get; set; }

    /// <summary>
    /// Gets or sets the OAuth strategy secret
    /// </summary>
    public string OAuthStrategySecret { get; set; }

    /// <summary>
    /// Gets or sets the OAuth strategy base URL
    /// </summary>
    public string OAuthStrategyBaseUrl { get; set; }
    
    
    /// <summary>
    /// Gets or sets the SMTP server settings for password resets
    /// </summary>
    public string SmtpServer { get; set; }
    
    /// <summary>
    /// Gets or sets the SMTP port settings for password resets
    /// </summary>
    public int SmtpPort { get; set; }
    /// <summary>
    /// Gets or sets the SMTP user settings for password resets
    /// </summary>
    public string SmtpUser { get; set; }
    /// <summary>
    /// Gets or sets the SMTP password settings for password resets
    /// </summary>
    public string SmtpPassword { get; set; }
    /// <summary>
    /// Gets or sets the email address emails are sent from
    /// </summary>
    public string SmtpSender { get; set; }

    /// <summary>
    /// Saves the system settings
    /// </summary>
    public void Save()
        => new SystemSettingsService().Save();

}