using Fenrus.Models;

namespace Fenrus.Workers;

/// <summary>
/// Worker to monitor users email
/// </summary>
public class MailWorker:Worker
{
    public static readonly MailWorker Instance;

    private readonly Dictionary<Guid, UserInbox> UserInboxes = new();
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

    protected override void Execute()
    {
        MonitorUsers();
    }

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

    public Task<List<EmailMessage>> GetLatest(Guid userUid)
    {
        if(UserInboxes.TryGetValue(userUid, out var inbox))
            return inbox.GetLatest();
        return Task.FromResult(new List<EmailMessage>());
    }

    public Task<EmailMessage?> GetByUid(Guid userUid, uint messageId)
    {
        if(UserInboxes.TryGetValue(userUid, out var inbox))
            return inbox.GetByUid(messageId);
        return Task.FromResult<EmailMessage?>(null);
        
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