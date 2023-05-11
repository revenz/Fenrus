using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using SixLabors.ImageSharp.Formats.Webp;

namespace Fenrus.Services;

/// <summary>
/// Service used to connect to an IMAP server
/// </summary>
public class ImapService : IDisposable
{
    private readonly Guid _userUid;
    private readonly string _server, _username, _password;
    private readonly int _port;
    private ImapClient _client, _clientWatcher;
    private CancellationTokenSource _canecellationToken;
    
    public delegate void RefreshHandler(Guid userUid);

    /// <summary>
    /// Event fired to refresh the inbox when it changes
    /// </summary>
    public event RefreshHandler Refresh;
    
    /// <summary>
    /// Constructs an instance of the IMAP service
    /// </summary>
    /// <param name="userUid">the UID of the user this IMAP server belongs to</param>
    /// <param name="server">the IMAP server address</param>
    /// <param name="port">the port used to connect to the server</param>
    /// <param name="username">the username used to for authentication</param>
    /// <param name="password">the password used to for authentication</param>
    public ImapService(Guid userUid, string server, int port, string username, string password)
    {
        if (port < 1 || port > 65535)
            port = 993;
        this._userUid = userUid;
        this._server = server;
        this._port = port;
        this._username = username;
        this._password = password;
    }

    /// <summary>
    /// Gets the IMAP client used for communication
    /// </summary>
    /// <returns>the IMAP client used for communication</returns>
    private async Task<ImapClient> GetClient()
    {
        _ = SetupWatcher();
        if (_client != null)
        {
            if( _client.IsConnected)
                return _client;
            _client.Dispose();
        }

        _client = new ImapClient();
        await _client.ConnectAsync(this._server, this._port);
        await _client.AuthenticateAsync(this._username, this._password);
        await _client.Inbox.OpenAsync(FolderAccess.ReadOnly);
        return _client;
    }

    private async Task SetupWatcher()
    {
        if (_clientWatcher != null)
        {
            if( _clientWatcher.IsConnected)
                return;
            _clientWatcher.Dispose();
        }
        _clientWatcher = new ImapClient();
        await _clientWatcher.ConnectAsync(this._server, this._port);
        await _clientWatcher.AuthenticateAsync(this._username, this._password);
        await _clientWatcher.Inbox.OpenAsync(FolderAccess.ReadOnly);
        _canecellationToken = new CancellationTokenSource();
        _ = _clientWatcher.IdleAsync(_canecellationToken.Token);
        _clientWatcher.Inbox.CountChanged += InboxOnCountChanged;
    }

    private void InboxOnCountChanged(object? sender, EventArgs e)
    {
        Refresh?.Invoke(this._userUid);
    }

    /// <summary>
    /// Gets the latest messages from the server
    /// </summary>
    /// <returns>the latest messages</returns>
    public async Task<List<EmailMessage>> GetLatest()
    {
        DateTime dtStart = DateTime.Now;
        var client = await GetClient();
        var timeInbox = DateTime.Now.Subtract(dtStart);
        Console.WriteLine("Time taken to open inbound: " + timeInbox);

        DateTime dt2 = DateTime.Now;

        lock (_client.SyncRoot)
        {
            var summaries = client.Inbox.Fetch(0, -1,
                    MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.InternalDate)
                .OrderByDescending(summary => summary.Date)
                .GroupBy(summary =>
                    summary.Envelope.MessageId ??
                    summary.Envelope.Subject) // group by message ID or subject if Message-ID is not available
                .Select(group =>
                    group.OrderByDescending(summary => summary.Date)
                        .First()) // select the newest message in each thread
                .Take(50)
                .Select(summary => EmailMessage.FromMessageSummary(summary))
                .ToList();

            var timeMsg = DateTime.Now.Subtract(dt2);
            var timeMsgTotal = DateTime.Now.Subtract(dtStart);
            Console.WriteLine("timeMsg: " + timeMsg);
            Console.WriteLine("timeMsgTotal: " + timeMsgTotal);

            return summaries;
        }
    }

    public async Task<EmailMessage?> GetByUid(uint uid)
    {
        var client = await GetClient();
        return await Task.Run(() =>
        {
            lock (_client.SyncRoot)
            {
                var message = client.Inbox.GetMessageAsync(new UniqueId(uid)).Result;
                return EmailMessage.FromMimeMessage(message).Result;
            }
        });
    }

    /// <summary>
    /// Tests the connection to the IMAP server
    /// </summary>
    /// <returns>a tuple with the success test result and an error message if unsuccessful</returns>
    public async Task<(bool Success, string Error)> Test()
    {
        try
        {
            await GetClient();;
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public void Dispose()
    {
        if (_client != null)
        {
            try
            {
                if (_client.IsConnected)
                {
                    _client.Inbox.Close();
                    _client.Disconnect(true);
                }

                _client.Dispose();
            }
            catch (Exception)
            {
            }
        }
        if (_clientWatcher != null)
        {
            _clientWatcher.Inbox.CountChanged -= InboxOnCountChanged; 
            try
            {
                _canecellationToken.Cancel();
                if (_clientWatcher.IsConnected)
                {
                    _clientWatcher.Inbox.Close();
                    _clientWatcher.Disconnect(true);
                }

                _clientWatcher.Dispose();
            }
            catch (Exception)
            {
            }
        }
    }
}

/// <summary>
/// Represents an email message with a subset of its properties for display in an inbox list.
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// Gets or sets the unique identifier of the email message.
    /// </summary>
    public uint Uid { get; set; }

    /// <summary>
    /// Gets or sets the message identifier of the email message.
    /// </summary>
    public string MessageId { get; set; }

    /// <summary>
    /// Gets or sets the sender of the email message.
    /// </summary>
    public string From { get; set; }

    /// <summary>
    /// Gets or sets the recipient(s) of the email message.
    /// </summary>
    public string[] To { get; set; }

    /// <summary>
    /// Gets or sets the subject of the email message.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets the date and time that the email message was sent, in UTC.
    /// </summary>
    public DateTime DateUtc { get; set; }

    /// <summary>
    /// Gets or sets the body of the email message.
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// Gets or sest if the body is HTML
    /// </summary>
    public bool IsHtml { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the email message has attachments.
    /// </summary>
    public bool HasAttachments { get; set; }

    /// <summary>
    /// Creates a new <see cref="EmailMessage"/> object from an <see cref="IMessageSummary"/> object.
    /// </summary>
    /// <param name="summary">The <see cref="IMessageSummary"/> object to create the email message from.</param>
    /// <returns>A new <see cref="EmailMessage"/> object with the properties extracted from the <paramref name="summary"/> object.</returns>
    public static EmailMessage FromMessageSummary(IMessageSummary summary)
    {
        return new EmailMessage
        {
            Uid = summary.UniqueId.Id,
            MessageId = summary.Envelope.MessageId,
            From = summary.Envelope.From?.ToString() ?? string.Empty,
            To = summary.Envelope.To?.Select(a => a.ToString()).ToArray() ?? new string[] { },
            Subject = summary.Envelope.Subject ?? string.Empty,
            DateUtc = summary.Date.UtcDateTime
        };
    }

    /// <summary>
    /// Converts a <see cref="MimeMessage"/> to an <see cref="EmailMessage"/>.
    /// </summary>
    /// <param name="message">The <see cref="MimeMessage"/> to convert.</param>
    /// <returns>The <see cref="EmailMessage"/> representation of the <see cref="MimeMessage"/>.</returns>
    public static async Task<EmailMessage> FromMimeMessage(MimeMessage message)
    {
        var emailMessage = new EmailMessage();

        if (message.Headers.Contains("X-Message-ID"))
            emailMessage.MessageId = message.Headers["X-Message-ID"];

        emailMessage.MessageId = emailMessage.MessageId?.EmptyAsNull() ?? message.MessageId;
        emailMessage.DateUtc = message.Date.UtcDateTime;
        emailMessage.From = message.From.ToString();
        emailMessage.To = message.To.Select(x => x.ToString()).ToArray();
        emailMessage.Subject = message.Subject;
        if(string.IsNullOrEmpty(message.HtmlBody))
            emailMessage.Body = message.TextBody ?? string.Empty;
        else
        {
            emailMessage.Body = CleanHtml(message);
            emailMessage.IsHtml = true;
        }

        emailMessage.HasAttachments = message.Attachments.Any();


        return emailMessage;
    }

    public static byte[] GetImageBytes(Image image, string contentType)
    {
        using (var stream = new MemoryStream())
        {
            image.Save(stream, new WebpEncoder());
            return stream.ToArray();
        }
    }

    private static string ImageToBase64Converted(Stream stream, string mimeType)
    {
        
        using (var image = Image.Load(stream))
        {
            // Resize the image if it's too large
            if (image.Width > 800 || image.Height > 800)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(800, 800),
                    Mode = ResizeMode.Max
                }));
            }

            // Convert the image to a Base64-encoded stringar contentType = imageAttachment.ContentType;
            try
            {
                var bytes = GetImageBytes(image, mimeType);
                string base64String = Convert.ToBase64String(bytes);

                return base64String;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }

    public static string CleanHtml(MimeMessage message)
    {

        var html = message.HtmlBody;
        if (message.Body is Multipart multipart == false)
            return html;

        
        foreach (var part in message.BodyParts.OfType<MimePart>())
        {
            if (string.IsNullOrEmpty(part.ContentId))
                continue;
            if (part.ContentDisposition?.Disposition != ContentDisposition.Inline)
                continue;
            
            using var memoryStream = new MemoryStream();
            part.Content.DecodeTo(memoryStream);
            memoryStream.Position = 0;
            string base64String = ImageToBase64Converted(memoryStream, part.ContentType.MediaSubtype);
            var cid = part.ContentId.Trim('<', '>');
            html = html.Replace($"cid:{cid}", $"data:webp;base64,{base64String}");
        }

        return html;
    }
}
