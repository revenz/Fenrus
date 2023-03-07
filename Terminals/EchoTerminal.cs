using System.Net.WebSockets;

namespace Fenrus.Terminals;

/// <summary>
/// Dummy terminal that just echos back to the user
/// </summary>
public class EchoTerminal : Terminal
{
    /// <summary>
    /// Constructs a terminal
    /// </summary>
    /// <param name="webSocket">the websocket this is reading/writing to</param>
    /// <param name="rows">the number of rows in this terminal</param>
    /// <param name="cols">the number of columns in this terminal</param>
    public EchoTerminal(WebSocket webSocket, int rows, int cols) : base(webSocket, rows, cols)
    {
    }
    
    /// <summary>
    /// Connects the terminal
    /// </summary>
    public override async Task Connect()
    {
        await SendMessage(DateTime.Now.ToString());

        var buffer = new byte[1024 * 4];
        var receiveResult = await this.Socket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            await this.Socket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);

            receiveResult = await this.Socket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await this.Socket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}