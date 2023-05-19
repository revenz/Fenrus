using System.Net.WebSockets;
using Docker.DotNet.Models;
using Renci.SshNet;

namespace Fenrus.Terminals;

/// <summary>
/// SSH terminal interface
/// </summary>
public class SshTerminal : Terminal
{
    /// <summary>
    /// Gets the docker server address
    /// </summary>
    private string Server { get; init; }
    /// <summary>
    /// Gets the docker server port
    /// </summary>
    public int Port { get; init; }
    
    /// <summary>
    /// Gets the SSH user name
    /// </summary>
    public string UserName{ get; private set; }
    
    /// <summary>
    /// Gets the SSH password
    /// </summary>
    public string Password { get; private set; }
    
    /// <summary>
    /// Constructs a new terminal
    /// </summary>
    /// <param name="webSocket">the websocket this is reading/writing to</param>
    /// <param name="rows">the number of rows in this terminal</param>
    /// <param name="cols">the number of columns in this terminal</param>
    /// <param name="server">the docker server address</param>
    /// <param name="port">the docker server port</param>
    /// <param name="username">the SSH username</param>
    /// <param name="password">the SSH password</param>
    public SshTerminal(WebSocket webSocket, int rows, int cols, string server, int port, string username, string password) :
        base(webSocket, rows, cols)
    {
        this.Server = server;
        this.Port = port < 1 || port > 65535 ? 22 : port;
        this.UserName = username;
        this.Password = password;
    }

    /// <summary>
    /// Connects the terminal
    /// </summary>
    public override async Task Connect()
    {
        using var client = await Login();
        try
        {
            var shell = client.CreateShellStream("xterm-256color", (uint)this.Columns, (uint)this.Rows, 0, 0, 10 * 1024);
          
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            _ = ReadOutputAsync(this.Socket, client, shell, source, token);

            await ReadInputAsync(this.Socket, shell, source, token);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in ssh: " + ex.Message + Environment.NewLine  + ex.StackTrace);
        }
    }

    /// <summary>
    /// Attempts to login to the SSH server
    /// </summary>
    public async Task<SshClient> Login()
    {
        string server = this.Server;
        int atIndex = server.IndexOf("@");
        string user = this.UserName;
        if (atIndex > 0)
        {
            if (string.IsNullOrEmpty(UserName))
                user = server[..atIndex];
            server = server[(atIndex + 1)..];
        }
        if (string.IsNullOrEmpty(this.UserName) == false)
        {
            SshClient? client = null;
            try
            {
                client = new SshClient(server, this.Port, this.UserName ?? string.Empty, this.Password ?? string.Empty);
                client.Connect();
                return client;
            }
            catch (Exception ex)
            {
                if(client != null)
                    client.Dispose();
                await SendMessage("Failed to login: " + ex.Message + "\n");
            }
        }

        if (string.IsNullOrEmpty(user))
            this.UserName = await RequestInput("User", false);
        else
            this.UserName = user;
        
        this.Password = await RequestInput("Password", true);
        return await Login();
    }


    /// <summary>
    /// Reads the output from the SSH socket
    /// </summary>
    /// <param name="webSocket">the browsers websocket</param>
    /// <param name="client">the ssh client</param>
    /// <param name="shellStream">the ssh shell stream</param>
    /// <param name="source">the cancellation token source</param>
    /// <param name="cancellationToken">the cancellation token</param>
    private async Task ReadOutputAsync(WebSocket webSocket, SshClient client, ShellStream shellStream, CancellationTokenSource source, CancellationToken cancellationToken = default)
    {
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            int i = 0;
            while (cancellationToken.IsCancellationRequested == false)
            {
                if ((i = await shellStream.ReadAsync(buffer, 0, buffer.Length, default)) != 0) //read data from the stream
                {
                    await SendMessage(client.ConnectionInfo.Encoding.GetString(buffer, 0, i));
                }
                // this is messy, but what they recommend, without this, the cpu is constantly hit and thrashed 
                Thread.Sleep(50);
                await shellStream.FlushAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            if(cancellationToken.IsCancellationRequested == false)
                source.Cancel(); 
            
            if (ex is ObjectDisposedException)
            {
                // happens when user exits the session
            }
            else
            {
                Logger.ELog("SSH terminal error: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
        Logger.ILog("SSH Terminal finished");
        System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
    }
    
    
    /// <summary>
    /// Reads the input from the browsers websocket
    /// </summary>
    /// <param name="webSocket">the browser websocket</param>
    /// <param name="shellStream">the ssh shell stream</param>
    /// <param name="source">the cancellation token source</param>
    /// <param name="cancellationToken">the cancellation token</param>
    private static async Task ReadInputAsync(WebSocket webSocket, ShellStream shellStream, CancellationTokenSource source , CancellationToken cancellationToken = default)
    {
        try
        {
            var wsBuffer = new byte[1024 * 4];
            while (true)
            {
                // read data from websocket input and write to the docker socket
                Array.Clear(wsBuffer, 0, wsBuffer.Length);
                var receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(wsBuffer), cancellationToken);
                if (shellStream == null || shellStream.CanWrite == false)
                    break;
                if (receiveResult.Count > 0)
                {
                    byte[] actual = wsBuffer[0..Array.FindIndex(wsBuffer, x => x == 0)];
                    if (actual.Any())
                    {
                        await shellStream.WriteAsync(actual, 0, actual.Length, cancellationToken);
                        await shellStream.FlushAsync(cancellationToken);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is ObjectDisposedException)
                return; // happens if the user ends the session
            if (ex is OperationCanceledException)
                return; // user closed terminal usually
            Logger.ELog("Failed reading stream input: " + ex.GetType().FullName + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
        }
    }
}