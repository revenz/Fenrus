using MailKit.Net.Smtp;
using MimeKit;

namespace Fenrus.Helpers;

/// <summary>
/// Emailer 
/// </summary>
public class Emailer
{
    /// <summary>
    /// Sends an email to the configured SMTP server
    /// </summary>
    /// <param name="recipient">the recipient of the email</param>
    /// <param name="subject">the subject of the email</param>
    /// <param name="plainTextBody">the plain text body of the email</param>
    public static void Send(string recipient, string subject, string plainTextBody)
    {
        var settings = new SystemSettingsService().Get();
        if (string.IsNullOrEmpty(settings.SmtpServer))
        {
            Logger.WLog("Cannot send email, no SMTP server configured");
            return;
        }
        Send(new EmailOptions()
        {
            Server = settings.SmtpServer,
            Port = settings.SmtpPort,
            Username = settings.SmtpUser,
            Password = EncryptionHelper.Decrypt(settings.SmtpPasswordEncrypted),
            Sender = settings.SmtpSender,
            Recipient = recipient,
            Subject = subject,
            PlainTextBody = plainTextBody
        });
    }

    /// <summary>
    /// Sends an email
    /// </summary>
    /// <param name="options">The email options</param>
    public static void Send(EmailOptions options)
    {
        var message = new MimeMessage();
        var sender = options.Sender?.Trim() ?? string.Empty;
        var recipient = options.Recipient?.Trim() ?? string.Empty;
        var server = options.Server?.Trim() ?? string.Empty;
        var user = options.Username?.Trim() ?? string.Empty;
        message.From.Add(new MailboxAddress(sender,sender));
        message.To.Add(new MailboxAddress(recipient, recipient));
        message.Subject = options.Subject ?? string.Empty;
        message.Body = new TextPart("plain")
        {
            Text = options.PlainTextBody ?? String.Empty
        };

        using var client = new SmtpClient();
        client.Connect(server, options.Port);

        if (string.IsNullOrEmpty(user) == false)
        {
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.Authenticate(options.Username, options.Password);
        }
        client.Send(message);
        client.Disconnect(true);
    }
}

/// <summary>
/// Email options
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// Gets or sets the SMTP server
    /// </summary>
    public string Server { get; set; }
    /// <summary>
    /// Gets or sets the SMTP port
    /// </summary>
    public int Port { get; set; }
    /// <summary>
    /// Gets or sets the SMTP username
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// Gets or sets the SMTP password
    /// </summary>
    public string Password { get; set; }
    /// <summary>
    /// Gets or sets the SMTP sender
    /// </summary>
    public string Sender { get; set; }
    /// <summary>
    /// Gets or sets the subject of the email 
    /// </summary>
    public string Subject { get; set; }
    /// <summary>
    /// Gets or sets the plain text body of the email
    /// </summary>
    public string PlainTextBody { get; set; }
    /// <summary>
    /// Gets or sets the recipient of the email
    /// </summary>
    public string Recipient { get; set; }
}