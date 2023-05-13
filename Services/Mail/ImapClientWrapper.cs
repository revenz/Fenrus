using MailKit;
using MailKit.Net.Imap;

namespace Fenrus.Services;

public class ImapClientWrapper : IDisposable
{
    private SemaphoreSlim _semaphoreSlim = new(1);
    private readonly Guid _userUid;
    private readonly string _server, _username, _password;
    private readonly int _port;
    private ImapClient client;
    private CancellationTokenSource _canecellationToken;
    CancellationTokenSource _idleCancellationTokenSource = null;
    private bool _disposed = false;
    
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
    public ImapClientWrapper(Guid userUid, string server, int port, string username, string password)
    {
        if (port < 1 || port > 65535)
            port = 993;
        this._userUid = userUid;
        this._server = server;
        this._port = port;
        this._username = username;
        this._password = password;
    }

    private void SetupWatch()
    {
        Task.Run(async () =>
        {
            while (_disposed == false)
            {
                await GetClient();
                await _semaphoreSlim.WaitAsync();
                try
                {
                    _idleCancellationTokenSource = new CancellationTokenSource();
                    var idleTask = client.IdleAsync(_idleCancellationTokenSource.Token);

                    // Wait for new messages to arrive or for a certain amount of time to pass
                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1), _idleCancellationTokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        // The token was canceled before the delay elapsed
                    }

                    // Stop IDLE mode
                    _idleCancellationTokenSource.Cancel();

                    // Wait for the IDLE task to complete
                    await idleTask;
                }
                finally
                {
                    _semaphoreSlim.Release();
                }

                await Task.Delay(3_000);
            }
        });
    }
    /// <summary>
    /// Gets the IMAP client used for communication
    /// </summary>
    /// <returns>the IMAP client used for communication</returns>
    private async Task<ImapClient> GetClient()
    {
        //_ = SetupWatcher();
        if (client != null)
        {
            if( client.IsConnected)
                return client;
            client.Dispose();
        }

        client = new ImapClient();
        await client.ConnectAsync(this._server, this._port);
        await client.AuthenticateAsync(this._username, this._password);
        await client.Inbox.OpenAsync(FolderAccess.ReadWrite);
        client.Inbox.CountChanged += InboxOnCountChanged;
        return client;
    }

    private void InboxOnCountChanged(object? sender, EventArgs e)
    {
        Refresh?.Invoke(this._userUid);
    }
    
    public async Task<ImapClientOperation> StartOperation()
    {
        var task = _semaphoreSlim.WaitAsync(); 
        if (_canecellationToken != null)
            _idleCancellationTokenSource.Cancel();
        await task;
        return new ImapClientOperation(await GetClient(), _semaphoreSlim);
    }

    public void Dispose()
    {
        _disposed = true;
        if (_canecellationToken != null)
            _idleCancellationTokenSource.Cancel();
        if (client != null)
        {
            try
            {
                if (client.IsConnected)
                {
                    client.Inbox.Close();
                    client.Disconnect(true);
                }

                client.Dispose();
            }
            catch (Exception)
            {
            }
        }
    }
}

public class ImapClientOperation : IDisposable
{
    private SemaphoreSlim _semaphoreSlim = new(1);
    public ImapClientOperation(ImapClient client, SemaphoreSlim semaphoreSlim)
    {
        this.Client = client;
        _semaphoreSlim = semaphoreSlim;
    }

    public void Dispose()
    {
        _semaphoreSlim.Release();
    }
    
    public ImapClient Client { get; init; }
}