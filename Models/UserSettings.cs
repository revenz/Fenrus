using Fenrus.Services;

namespace Fenrus.Models;

/// <summary>
/// User settings
/// </summary>
public class UserSettings: IModal
{
    /// <summary>
    /// Gets the Uid of the User
    /// </summary>
    [LiteDB.BsonId]
    public Guid Uid { get; set; }

    /// <summary>
    /// Gets the users uid, which is is the same as this UID
    /// </summary>
    [LiteDB.BsonIgnore]
    public Guid UserUid => Uid;

    /// <summary>
    /// Gets or sets the users name
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the language for this user
    /// </summary>
    public string Language { get; set; }
    
    /// <summary>
    /// Gets or sets the users theme settings
    /// </summary>
    public Dictionary<string, Dictionary<string, object?>> ThemeSettings { get; set; }
    
    /// <summary>
    /// Gets or sets the group UIDs
    /// </summary>
    public List<Guid> GroupUids { get; set; }
    /// <summary>
    /// Gets or sets the dashboard UIDs
    /// </summary>
    public List<Guid> DashboardUids { get; set; }
    /// <summary>
    /// Gets or sets the search engine UIDs
    /// </summary>
    public List<Guid> SearchEngineUids { get; set; }
    
    /// <summary>
    /// Saves the user settings
    /// </summary>
    public void Save()
    {
        if(this.Uid == Guid.Empty)
            return; // dont save if Guid is empty, this means it is likely the guest dashboard
        
        new UserSettingsService().Save(this);
    }
}