namespace Fenrus.Models;

/// <summary>
/// A note taken by the user
/// </summary>
public class Note : IModal, IUserModal
{
    /// <summary>
    /// Gets or sets the UID of this note
    /// </summary>
    [LiteDB.BsonId]
    public Guid Uid { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the note
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the UID of the owner of this note
    /// </summary>
    public Guid UserUid { get; set; }
    
    /// <summary>
    /// Gets or sets the UID of the dashboard this note belongs to
    /// </summary>
    public Guid DashboardUid { get; set; }
    
    /// <summary>
    /// Gets or sets the date the note was taken
    /// </summary>
    public DateTime Created { get; set; }
    
    /// <summary>
    /// Gets or sets the content of the note
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Gets or sets the order of the note
    /// </summary>
    public int Order { get; set; }
}