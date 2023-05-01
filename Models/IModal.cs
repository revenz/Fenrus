using LiteDB;

namespace Fenrus.Models;


/// <summary>
/// Basic modal with no name and just a UID
/// </summary>
public interface IModalUid
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    [BsonId]
    Guid Uid { get; set; }
}

/// <summary>
/// Modal interface
/// </summary>
public interface IModal : IModalUid
{
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    string Name { get; set; }
}

/// <summary>
/// Interface for user items
/// </summary>
public interface IUserModal
{
    /// <summary>
    /// Gets or sets the users UID
    /// </summary>
    Guid UserUid { get; set; }
}