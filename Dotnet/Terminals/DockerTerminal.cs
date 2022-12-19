using System.Net.WebSockets;
using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Fenrus.Terminals;

/// <summary>
/// Docker terminal interface
/// </summary>
public class DockerTerminal: Terminal
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
    /// Gets the dockers container name
    /// </summary>
    public string ContainerName { get; init; }
    
    /// <summary>
    /// Constructs a new terminal
    /// </summary>
    /// <param name="webSocket">the websocket this is reading/writing to</param>
    /// <param name="rows">the number of rows in this terminal</param>
    /// <param name="cols">the number of columns in this terminal</param>
    /// <param name="server">the docker server address</param>
    /// <param name="port">the docker server port</param>
    /// <param name="containerName">the dockers container name</param>
    public DockerTerminal(WebSocket webSocket, int rows, int cols, string server, int port, string containerName) :
        base(webSocket, rows, cols)
    {
        this.Server = server;
        this.Port = port < 1 || port > 65535 ? 2375 : port;
        this.ContainerName = containerName;
    }

    /// <summary>
    /// Connects the terminal
    /// </summary>
    public override async Task Connect()
    {
        DockerClient client = new DockerClientConfiguration(new Uri($"http://{Server}:{(Port)}")).CreateClient();

        var exec = await client.Exec.ExecCreateContainerAsync(ContainerName, new()
        {
            Cmd = new List<string>() {"/bin/bash"},
            AttachStderr = true,
            AttachStdin = true,
            AttachStdout = true,
            Tty = true,
            Env = new[] { $"LINES={this.Rows}", $"COLUMNS={this.Columns}" }
        });
        
        using var multiplexedStream = await client.Exec.StartWithConfigContainerExecAsync(exec.ID,new ContainerExecStartParameters { Detach=false,Tty=false}, default);
        
        CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken token = source.Token;
        _ = ReadOutputAsync(this.Socket, multiplexedStream, source, token);

        await ReadInputAsync(this.Socket, multiplexedStream, source, token);
        
        await this.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
    }
    
    
    /// <summary>
    /// Reads the output from the docker shell
    /// </summary>
    /// <param name="webSocket">the browsers websocket</param>
    /// <param name="multiplexedStream">the docker shell stream</param>
    /// <param name="source">the cancellation token source</param>
    /// <param name="cancellationToken">the cancellation token</param>
    private async Task ReadOutputAsync(WebSocket webSocket, MultiplexedStream multiplexedStream, CancellationTokenSource source, CancellationToken cancellationToken = default)
    {
        var dockerBuffer = System.Buffers.ArrayPool<byte>.Shared.Rent(81920);
        try
        {
            while (true)
            {
                // Clear buffer
                Array.Clear(dockerBuffer, 0, dockerBuffer.Length);
                var dockerReadResult = await multiplexedStream.ReadOutputAsync(dockerBuffer, 0, dockerBuffer.Length, cancellationToken);

                if (dockerReadResult.EOF)
                {
                    source.Cancel();
                    break;
                }

                if (dockerReadResult.Count > 0)
                {
                    string str = Encoding.UTF8.GetString(dockerBuffer, 0, dockerReadResult.Count);
                    Console.WriteLine(str);
                    await SendMessage(str);
                }
                else
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.ELog("Docker container error: " + ex.Message + Environment.NewLine + ex.StackTrace);
            //_logger.LogError(ex, "Failure during Read from Docker Exec to WebSocket");
        }
        Logger.ILog("Docker container finished");
        System.Buffers.ArrayPool<byte>.Shared.Return(dockerBuffer);
    }

    /// <summary>
    /// Reads the input from the browsers websocket
    /// </summary>
    /// <param name="webSocket">the browser websocket</param>
    /// <param name="multiplexedStream">the docker shell stream</param>
    /// <param name="source">the cancellation token source</param>
    /// <param name="cancellationToken">the cancellation token</param>
    private static async Task ReadInputAsync(WebSocket webSocket, MultiplexedStream multiplexedStream, CancellationTokenSource source , CancellationToken cancellationToken = default)
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
                if (receiveResult.Count > 0)
                {
                    byte[] actual = wsBuffer[0..Array.FindIndex(wsBuffer, x => x == 0)];
                    if (actual.Any())
                    {
                        await multiplexedStream.WriteAsync(actual, 0, actual.Length, cancellationToken);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.ELog("Failed reading docker stream input: " + ex.GetType().FullName + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
        }
    }
}