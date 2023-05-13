using Fenrus.Models;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Fenrus.Services;

/// <summary>
/// Service used to connect to an IMAP server
/// </summary>
public class ImapService : IDisposable
{
    private UserProfile _profile;
    //private ImapClient client, _clientWatcher;

    /// <summary>
    /// Gets if this server is a gmail server
    /// </summary>
    private bool IsGmail => _profile?.EmailImapServer?.Value?.ToLowerInvariant().Contains("gmail") == true 
                            || _profile?.EmailImapServer?.Value?.Contains("google") == true;

    /// <summary>
    /// Gets the name of the archive folder
    /// </summary>
    private string ArchiveFolderName => IsGmail ? "All Mail" : "Archive";
    
    /// <summary>
    /// A delegate that is used to trigger a refresh on the client
    /// </summary>
    public delegate void RefreshHandler(Guid userUid);

    /// <summary>
    /// Event fired to refresh the inbox when it changes
    /// </summary>
    public event RefreshHandler Refresh;

    private ImapClientWrapper clientHelper;
    
    /// <summary>
    /// Constructs an instance of the IMAP service
    /// </summary>
    /// <param name="profile">the profile of the user this is for</param>
    public ImapService(UserProfile profile)
    {
        this.clientHelper = new(profile.Uid, profile.EmailImapServer, profile.EmailImapPort, profile.EmailImapUsername, profile.EmailImapPassword);
        this.clientHelper.Refresh += (Guid uid) => { Refresh?.Invoke(uid); };
    }

    /// <summary>
    /// Gets the latest messages from the server
    /// </summary>
    /// <returns>the latest messages</returns>
    public async Task<List<EmailMessage>> GetLatest()
    {
        using var operation = await clientHelper.StartOperation();
        var client = operation.Client;
        DateTime dtStart = DateTime.Now;
        var timeInbox = DateTime.Now.Subtract(dtStart);
        Console.WriteLine("Time taken to open inbound: " + timeInbox);

        DateTime dt2 = DateTime.Now;

        var summaries = (await client.Inbox.FetchAsync(0, -1,
                MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.InternalDate | MessageSummaryItems.Flags | MessageSummaryItems.GMailLabels | MessageSummaryItems.Annotations)
            )
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

    /// <summary>
    /// Gets a message by its UID
    /// </summary>
    /// <param name="uid">the UID of the message</param>
    /// <returns>the message if found, otherwise null</returns>
    public async Task<EmailMessage?> GetByUid(uint uid)
    {
        using var operation = await clientHelper.StartOperation();
        var client = operation.Client;
        var message = await client.Inbox.GetMessageAsync(new UniqueId(uid));
        await client.Inbox.AddFlagsAsync(new UniqueId(uid), MessageFlags.Seen, true);
        return EmailMessage.FromMimeMessage(message).Result;
    }

    /// <summary>
    /// Marks a message as read
    /// </summary>
    /// <param name="uid">the UID of the message</param>
    public async Task MarkAsRead(uint uid)
    {
        using var operation = await clientHelper.StartOperation();
        var client = operation.Client;
        await client.Inbox.AddFlagsAsync(new UniqueId(uid), MessageFlags.Seen, true);
    }

    /// <summary>
    /// Archives a message
    /// </summary>
    /// <param name="uid">the UID of the message</param>
    public async Task Archive(uint uid)
    {
        using var operation = await clientHelper.StartOperation();
        var client = operation.Client;
        IMailFolder? archiveFolder = (await client.GetFoldersAsync(client.PersonalNamespaces[0]))
                .FirstOrDefault(x => x.Name == ArchiveFolderName);

        if (archiveFolder == null)
            throw new Exception("Could not find archive folder: " + ArchiveFolderName);
        
        await archiveFolder.OpenAsync(FolderAccess.ReadWrite);
        await client.Inbox.OpenAsync(FolderAccess.ReadWrite);
        await client.Inbox.MoveToAsync(new UniqueId(uid), archiveFolder);
    }

    /// <summary>
    /// Deletes a message
    /// </summary>
    /// <param name="uid">the UID of the message</param>
    public async Task Delete(uint uid)
    {
        using var operation = await clientHelper.StartOperation();
        var client = operation.Client;
        await client.Inbox.AddFlagsAsync(new UniqueId(uid), MessageFlags.Deleted, true);
        await client.Inbox.ExpungeAsync();
    }
    
    /// <summary>
    /// Replies to the specified message, including any attachments, using best practices for email replies.
    /// </summary>
    /// <param name="uid">The UID of the message to reply to.</param>
    /// <param name="body">The body of the reply email.</param>
    /// <param name="attachments">A list of attachments to include with the reply email.</param>
    /// <param name="all">True to reply to all recipients, false to reply to sender only.</param>
    public async Task ReplyToMessage(uint uid, string body, List<MailAttachment> attachments, bool all)
    {
        // Get the message
        MimeMessage message;
        
        // converts all \n to \r\n to match the internet standard for emails
        body = body.Replace("\r\n", "\n").Replace("\n", "\r\n");

        using (var operation = await clientHelper.StartOperation())
        {
            var client = operation.Client;
            message = await client.Inbox.GetMessageAsync(new UniqueId(uid));
        }

        // Create the reply message
        var reply = new MimeMessage();
        reply.Subject = $"Re: {message.Subject}";
        reply.To.Add(new MailboxAddress(all ? message.From.Mailboxes.FirstOrDefault()?.Name : null, 
            all ? message.From.Mailboxes.FirstOrDefault()?.Address : null));
        if (all)
        {
            reply.Cc.AddRange(message.Cc);
            reply.Bcc.AddRange(message.Bcc);
        }
        reply.InReplyTo = message.MessageId;
        foreach(var reference in message.References)
            reply.References.Add(reference);

        // Create the body of the reply
        var builder = new BodyBuilder
        {
            HtmlBody = body
        };

        // Add any attachments
        foreach (var attachment in attachments)
        {
            builder.Attachments.Add(attachment.Name, attachment.Data);
        }

        // Include original message body using best practices
        builder.TextBody += Environment.NewLine + Environment.NewLine;
        builder.TextBody += "On " + message.Date.ToString("f") + ", " + message.From + " wrote:" + Environment.NewLine;
        using (var reader = new StringReader(message.TextBody))
        {
            while (await reader.ReadLineAsync() is { } line)
            {
                builder.TextBody += "> " + line + "\r\n"; // standard to use \r\n instead of \n for emails
            }
        }

        // Set the body of the reply
        reply.Body = builder.ToMessageBody();

        // Send the reply message

        using (var smtpClient = GetSmtpClient())
        {
            await smtpClient.SendAsync(reply);
            await smtpClient.DisconnectAsync(true);
        }
    }

    /// <summary>
    /// Gets the SMTP client used to send messages
    /// </summary>
    /// <returns>the SMTP client used to send messages</returns>
    private SmtpClient GetSmtpClient()
    {
        var smtpClient = new SmtpClient();
        int port = _profile.EmailSmtpPort is < 1 or > 65535 ? 587 : _profile.EmailSmtpPort;
        smtpClient.Connect(_profile.EmailSmtpServer, port, SecureSocketOptions.StartTls);
        smtpClient.Authenticate(_profile.EmailSmtpUsername,  _profile.EmailSmtpPassword);
        return smtpClient;
    }


    /// <summary>
    /// Tests the connection to the IMAP server
    /// </summary>
    /// <returns>a tuple with the success test result and an error message if unsuccessful</returns>
    public async Task<(bool Success, string Error)> Test()
    {
        try
        {
            using var operation = await clientHelper.StartOperation();
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Disposes of this object
    /// </summary>
    public void Dispose()
    {
        clientHelper.Dispose();
    }
}
