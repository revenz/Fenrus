using System.Net.WebSockets;
using System.Text;

namespace Fenrus.Terminals;

/// <summary>
/// Terminal interface
/// </summary>
public abstract class Terminal
{
    /// <summary>
    /// Gets or sets the number of rows in this terminal
    /// </summary>
    protected int Rows { get; set; }
    
    /// <summary>
    /// Gets or sets the number of columns in this terminal
    /// </summary>
    protected int Columns { get; set; }

    /// <summary>
    /// Gets or sets the websocket this is reading/writing to
    /// </summary>
    protected WebSocket Socket { get; set; }
    
    /// <summary>
    /// Constructs a new terminal
    /// </summary>
    /// <param name="webSocket">the websocket this is reading/writing to</param>
    /// <param name="rows">the number of rows in this terminal</param>
    /// <param name="cols">the number of columns in this terminal</param>
    public Terminal(WebSocket webSocket, int rows, int cols)
    {
        this.Socket = webSocket;
        this.Rows = rows;
        this.Columns = cols;
    }

    /// <summary>
    /// Connects the terminal
    /// </summary>
    public abstract Task Connect();
    
    
    /// <summary>
    /// Sends a message to the client over hte websocket
    /// </summary>
    /// <param name="message">the message to send</param>
    protected async Task SendMessage(string message)
    {
        if (this.Socket.State != WebSocketState.Open) 
            return; 
        var bytes = Encoding.UTF8.GetBytes(message);
        var buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);
        await this.Socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    /// <summary>
    /// Requests input from the user
    /// </summary>
    /// <param name="field">the name of the field to request, eg user</param>
    /// <param name="hidden">if the field should be hidden or echoed back to the user</param>
    /// <returns>the value entered by the user</returns>
    protected async Task<string> RequestInput(string field, bool hidden)
    {
        await SendMessage("\x1b[1;32m" + field + ":\x1b[37m ");
        
        var buffer = new byte[1024 * 4];
        var receiveResult = await this.Socket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        string value = string.Empty;
        int bsCount = 0;
        while (!receiveResult.CloseStatus.HasValue)
        {
            if (buffer[0] == 127)
            {
                ++bsCount;
                if (value.Length > 0)
                {
                    value = value[..^1];
                    await SendMessage("\b \b");
                }
                else if (bsCount > 2)
                {
                    bsCount = 0;
                    await HandleBackspaceCleared(field);
                }
            }
            else if(buffer[0] == 13)
            {
                break;
            }
            else
            {
                bsCount = 0;
                if (hidden == false)
                {
                    await this.Socket.SendAsync(
                        new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                        receiveResult.MessageType,
                        receiveResult.EndOfMessage,
                        CancellationToken.None);
                }

                value += Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
            }

            receiveResult = await this.Socket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await SendMessage("\n");

        return value.Trim();
    }

    /// <summary>
    /// Called if the user presses backspace 3 times on an empty input
    /// </summary>
    /// <param name="field">the currently being asked for</param>
    /// <returns>a task to await</returns>
    protected virtual Task HandleBackspaceCleared(string field)
    {
        return Task.CompletedTask;
    }
}