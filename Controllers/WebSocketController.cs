using System.Net.WebSockets;
using System.Text;
using Fenrus.Workers;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;


/// <summary>
/// Controller for the websocket
/// </summary>
[Route("websocket")]
[Authorize]
public class WebSocketController : BaseController
{
    /// <summary>
    /// Gets the websocket
    /// </summary>
    [HttpGet]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest == false)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }
        var uid = User.GetUserUid();
        if (uid == null || uid.Value == Guid.Empty)
        {
            HttpContext.Response.StatusCode = 401;
            return;
        }

        // var timer = new System.Timers.Timer();
        // timer.Interval = 10_000;
        // timer.AutoReset = true;
        // timer.Elapsed += (sender, args) =>
        // {
        //     NotificationHelper.Send(uid.Value, NotificationType.Info, "a title", "a message");
        // };
        // timer.Start();

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var handler = new FenrusSocketHandler(webSocket, uid.Value);
        await handler.WaitForClose();
        handler.Dispose();
        // timer.Stop();
        // timer.Dispose();

    }
}

/// <summary>
/// A Fenrus Socket handler that handles a websocket from the client and sends it information when needed
/// </summary>
public class FenrusSocketHandler:IDisposable
{
    private readonly WebSocket _socket;
    private readonly Guid _userUid;
    
    /// <summary>
    /// Constructs an instance of a Fenrus Socket Handler
    /// </summary>
    /// <param name="socket">the socket to handle</param>
    /// <param name="userUid">the UID of the user</param>
    public FenrusSocketHandler(WebSocket socket, Guid userUid)
    {
        this._socket = socket;
        this._userUid = userUid;
        CalendarEventWorker.RegisterClient();
        NotificationHelper.NotificationReceived += NotificationHelperOnNotificationReceived; 
        MailWorker.Instance.EmailEvent += InstanceOnEmailEvent;
    }

    private void InstanceOnEmailEvent(Guid useruid, string eventname, object data)
    {
        if (useruid != this._userUid)
            return;
        _ = Send("email", JsonSerializer.Serialize(
        new {
           @event = eventname,
           data
        }));
    }

    /// <summary>
    /// Called when a notification is received
    /// </summary>
    /// <param name="notification">the notification</param>
    private void NotificationHelperOnNotificationReceived(Notification notification)
    {
        if (notification.UserUid != Guid.Empty && notification.UserUid != _userUid)
            return;

        _ = Send("notification", JsonSerializer.Serialize(new
        {
            type = notification.Type.ToString().ToLower(),
            title = notification.Title,
            message = notification.Message,
            duration = notification.Duration,
            identifier = notification.Identifier
        }));
    }

    /// <summary>
    /// Sends a message to the client
    /// </summary>
    /// <param name="type">the type of message to send</param>
    /// <param name="data">the data of the message</param>
    private async Task Send(string type, string data)
    {
        MemoryStream ms = new MemoryStream();
        using var writer = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);
        writer.Write(JsonSerializer.Serialize(new {type,data}));
        writer.Flush();
        ms.Position = 0;
        ArraySegment<byte> byteArray = new ArraySegment<byte>(ms.ToArray());
        await _socket.SendAsync(byteArray, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    /// <summary>
    /// Waits for the socket to close
    /// </summary>
    public async Task WaitForClose()
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult receiveResult;
        do
        {

            receiveResult = await _socket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        } while (!receiveResult.CloseStatus.HasValue);
        
        await _socket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }

    /// <summary>
    /// Disposes of this object
    /// </summary>
    public void Dispose()
    {
        CalendarEventWorker.UnregisterClient();
        NotificationHelper.NotificationReceived -= NotificationHelperOnNotificationReceived;
    }
}