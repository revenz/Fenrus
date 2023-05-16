using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace Fenrus.Middleware
{
    /// <summary>
    /// Helper class for processing WebSocket connections.
    /// </summary>
    public static class WebSocketHelper
    {
        private const int ReceiveChunkSize = 1024;

        /// <summary>
        /// Processes the WebSocket connection, reading data from the WebSocket and writing it to the network stream.
        /// </summary>
        /// <param name="webSocket">The WebSocket instance to process.</param>
        /// <param name="networkStream">The network stream to write data to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the read operation.</param>
        public static async Task ProcessWebSocketAsync(
            WebSocket webSocket,
            NetworkStream networkStream,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var receiveBuffer = new byte[ReceiveChunkSize];
                WebSocketReceiveResult receiveResult;

                do
                {
                    receiveResult = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(receiveBuffer),
                        cancellationToken);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            string.Empty,
                            cancellationToken);
                    }
                    else if (receiveResult.MessageType == WebSocketMessageType.Binary)
                    {
                        await networkStream.WriteAsync(
                            receiveBuffer,
                            0,
                            receiveResult.Count,
                            cancellationToken);
                    }
                    else if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(
                            receiveBuffer,
                             0,
                             receiveResult.Count);

                        await networkStream.WriteAsync(
                            Encoding.UTF8.GetBytes(message),
                            0,
                            message.Length,
                            cancellationToken);
                    }
                } while (!receiveResult.CloseStatus.HasValue);
            }
            catch (WebSocketException ex)
            {
                if (ex.WebSocketErrorCode != WebSocketError.ConnectionClosedPrematurely)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Processes the network stream, reading data from the network stream and writing it to the WebSocket.
        /// </summary>
        /// <param name="webSocket">The WebSocket instance to write data to.</param>
        /// <param name="networkStream">The network stream to read data from.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the write operation.</param>
        public static async Task ProcessNetworkAsync(
            WebSocket webSocket,
            NetworkStream networkStream,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var receiveBuffer = new byte[ReceiveChunkSize];

                while (webSocket.State == WebSocketState.Open)
                {
                    var bytesRead = await networkStream.ReadAsync(
                        receiveBuffer,
                        0,
                        ReceiveChunkSize,
                        cancellationToken);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    await webSocket.SendAsync(
                        new ArraySegment<byte>(receiveBuffer, 0, bytesRead),
                        WebSocketMessageType.Binary,
                        true,
                        cancellationToken);
                }
            }
            catch (WebSocketException ex)
            {
                if (ex.WebSocketErrorCode != WebSocketError.ConnectionClosedPrematurely)
                {
                    throw;
                }
            }
        }
    }
}
