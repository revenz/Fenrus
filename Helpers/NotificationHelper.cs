namespace Fenrus.Helpers;

/// <summary>
/// Notification helper
/// </summary>
public class NotificationHelper
{
    /// <summary>
    /// Sends a notification to a user
    /// </summary>
    /// <param name="userUid">the UID of the user to send the notification to, use Guid.Empty for all users</param>
    /// <param name="type">the type of notification to send</param>
    /// <param name="title">the title of the notification</param>
    /// <param name="message">the message of the notification</param>
    /// <param name="duration">the number of seconds to show the notification for</param>
    /// <param name="identifier">optional identifier for the message to prevent this notification from being seen multiple times</param>
    public static void Send(Guid userUid, NotificationType type, string title, string message, int duration = 10, string? identifier = null)
    {
        NotificationReceived?.Invoke(new ()
        {
            UserUid = userUid,
            Type = type,
            Title = title,
            Message = message,
            Duration = duration,
            Identifier = identifier
        });
    }

    /// <summary>
    /// The delegate for the notification received event
    /// </summary>
    public delegate void NotificationReceivedEvent(Notification notification);

    /// <summary>
    /// A event that is fired when a notification has been received
    /// </summary>
    public static event NotificationReceivedEvent NotificationReceived;
}

/// <summary>
/// A notification to send to the user
/// </summary>
public class Notification
{
    /// <summary>
    /// Get or sets the UID of the user to send the notification to, use Guid.Empty for all users
    /// </summary>
    public Guid UserUid { get; set; }
    /// <summary>
    /// Get or sets the type of notification to send
    /// </summary>
    public NotificationType Type { get; set; }
    /// <summary>
    /// Get or sets the title of the notification
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// Get or sets the message of the notification
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// Gets or sets the number of seconds to show the notification for
    /// </summary>
    public int Duration { get; set; }
    
    /// <summary>
    /// Gets or sets optional identifier for the message to prevent this notification from being seen multiple times
    /// </summary>
    public string? Identifier { get; set; }
}


/// <summary>
/// Types of notifications
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// A success notification
    /// </summary>
    Success = 1,
    /// <summary>
    /// A information notification
    /// </summary>
    Info = 2,
    /// <summary>
    /// A warning notification
    /// </summary>
    Warning = 3,
    /// <summary>
    /// A error notification
    /// </summary>
    Error = 4
}