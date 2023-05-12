using Fenrus.Models;

namespace Fenrus.Workers;

/// <summary>
/// Worker to monitor users email
/// </summary>
public class MailWorker:Worker
{
    /// <summary>
    /// Gets the singleton instance of this worker
    /// </summary>
    public static readonly MailWorker Instance;

    private readonly Dictionary<Guid, UserInbox> UserInboxes = new();
    /// <summary>
    /// An event that is fired regarding emails
    /// </summary>
    public delegate void EmailEventHandler(Guid userUid, string eventName, object data);

    /// <summary>
    /// Event fired to refresh the inbox when it changes
    /// </summary>
    public event EmailEventHandler EmailEvent;

    /// <summary>
    /// Static constructor
    /// </summary>
    static MailWorker()
    {
        Instance = new();
    }

    /// <summary>
    /// Constructs a mail worker
    /// </summary>
    private MailWorker() : base(ScheduleType.Hourly, 2)
    {
        MonitorUsers();
    }

    /// <summary>
    /// Called when worker is triggered
    /// </summary>
    protected override void Execute()
        => MonitorUsers();

    /// <summary>
    /// Sets up monitors for configured users email
    /// </summary>
    private void MonitorUsers()
    {
        var profiles = DbHelper.GetAll<UserProfile>();
        foreach (var profile in profiles)
        {
            if (string.IsNullOrEmpty(profile.EmailServer))
                continue;
            UserInboxes[profile.Uid] = new UserInbox(profile);
            UserInboxes[profile.Uid].ImapService.Refresh += ImapServiceOnRefresh;
        }
    }

    /// <summary>
    /// Setups a mail server watcher for a users profile
    /// </summary>
    /// <param name="profile">the profile o fhte user</param>
    public void UserMailServer(UserProfile profile)
    {
        if (UserInboxes.ContainsKey(profile.Uid))
        {
            // need to dispose of old and create a new one
            UserInboxes[profile.Uid].Dispose();
            UserInboxes.Remove(profile.Uid);
            UserInboxes[profile.Uid].ImapService.Refresh -= ImapServiceOnRefresh;
        }

        if (string.IsNullOrEmpty(profile.EmailServer))
            return;

        UserInboxes[profile.Uid] = new UserInbox(profile);
        UserInboxes[profile.Uid].ImapService.Refresh += ImapServiceOnRefresh;
    }

    private void ImapServiceOnRefresh(Guid userUid)
    {
        EmailEvent?.Invoke(userUid, "refresh", null);
    }

    /// <summary>
    /// Gets the latest emails for a user
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <returns>the latest emails</returns>
    public Task<List<EmailMessage>> GetLatest(Guid userUid)
    {
        if(UserInboxes.TryGetValue(userUid, out var inbox))
            return inbox.GetLatest();
        return Task.FromResult(new List<EmailMessage>());
    }

    /// <summary>
    /// Gets a message by its UID
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="uid">the message UID</param>
    /// <returns>the message</returns>
    public Task<EmailMessage?> GetByUid(Guid userUid, uint uid)
    {
        if(UserInboxes.TryGetValue(userUid, out var inbox))
            return inbox.GetByUid(uid);
        return Task.FromResult<EmailMessage?>(null);
    }
    
    /// <summary>
    /// Marks a message as read
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="uid">the UID of the message</param>
    public async Task MarkAsRead(Guid userUid, uint uid)
    {
        if (UserInboxes.TryGetValue(userUid, out var inbox) == false)
            return;
        await inbox.ImapService.MarkAsRead(uid);
    }

    /// <summary>
    /// Archives a message
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="uid">the UID of the message</param>
    public async Task Archive(Guid userUid, uint uid)
    {
        if (UserInboxes.TryGetValue(userUid, out var inbox) == false)
            return;
        await inbox.ImapService.Archive(uid);
    }

    /// <summary>
    /// Deletes a message
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="uid">the UID of the message</param>
    public async Task Delete(Guid userUid, uint uid)
    {
        if (UserInboxes.TryGetValue(userUid, out var inbox) == false)
            return;
        await inbox.ImapService.Delete(uid);
    }

    private class UserInbox: IDisposable
    {
        public ImapService ImapService { get; set; }


        public UserInbox(UserProfile profile)
        {
            ImapService = new(profile.Uid, profile.EmailServer, profile.EmailPort, profile.EmailUsername, profile.EmailPassword);
        }

        public Task<List<EmailMessage>> GetLatest()
            => ImapService.GetLatest();
        public Task<EmailMessage?> GetByUid(uint uid)
            => ImapService.GetByUid(uid);


        public void Dispose()
        {
            ImapService.Dispose();
        }
    }
}