using System.Net.Sockets;
using System.Text;
using Fenrus.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for the websocket
/// </summary>
[Route("websockify")]
[Authorize]
public class NoVncController:BaseController
{

    /// <summary>
    /// Gets the websocket
    /// </summary>
    [HttpGet("{server}/{port?}")]
    public async Task Get([FromRoute] string server, [FromRoute] int port = 5900)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest == false)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }

        // string password = "pass";

        var uid = User.GetUserUid();
        if (uid == null || uid.Value == Guid.Empty)
        {
            HttpContext.Response.StatusCode = 401;
            return;
        }

        try
        {

            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            using var tcpClient = new TcpClient(server, port);
            using var networkStream = tcpClient.GetStream();

            // Send the password to the VNC server
            // if (string.IsNullOrEmpty(password) == false)
            // {
            //     byte[] passwordBytes = Encoding.ASCII.GetBytes(password + "\n");
            //     await networkStream.WriteAsync(passwordBytes, 0, passwordBytes.Length);
            // }

            await Task.WhenAll(
                WebSocketHelper.ProcessWebSocketAsync(webSocket, networkStream),
                WebSocketHelper.ProcessNetworkAsync(webSocket, networkStream)
            );
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}