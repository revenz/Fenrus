using MailKit;
using MailKit.Net.Imap;
using Microsoft.ClearScript.Util.Web;
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

    /// <summary>
    /// Gets if this server is a gmail server
    /// </summary>
    private bool IsGmail => _server.ToLowerInvariant().Contains("gmail") || _server.ToLowerInvariant().Contains("google");

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
        await _client.Inbox.OpenAsync(FolderAccess.ReadWrite);
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
                    MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.InternalDate | MessageSummaryItems.Flags)
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

    /// <summary>
    /// Gets a message by its UID
    /// </summary>
    /// <param name="uid">the UID of the message</param>
    /// <returns>the message if found, otherwise null</returns>
    public async Task<EmailMessage?> GetByUid(uint uid)
    {
        var client = await GetClient();
        return await Task.Run(() =>
        {
            lock (_client.SyncRoot)
            {
                var message = client.Inbox.GetMessageAsync(new UniqueId(uid)).Result;
                client.Inbox.AddFlagsAsync(new UniqueId(uid), MessageFlags.Seen, true);
                return EmailMessage.FromMimeMessage(message).Result;
            }
        });
    }

    /// <summary>
    /// Marks a message as read
    /// </summary>
    /// <param name="uid">the UID of the message</param>
    public async Task MarkAsRead(uint uid)
    {
        var client = await GetClient();
        await client.Inbox.AddFlagsAsync(new UniqueId(uid), MessageFlags.Seen, true);
    }

    /// <summary>
    /// Archives a message
    /// </summary>
    /// <param name="uid">the UID of the message</param>
    public async Task Archive(uint uid)
    {
        var client = await GetClient();
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
        var client = await GetClient();
        await client.Inbox.AddFlagsAsync(new UniqueId(uid), MessageFlags.Deleted, true);
        await client.Inbox.ExpungeAsync();
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

    /// <summary>
    /// Disposes of this object
    /// </summary>
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
