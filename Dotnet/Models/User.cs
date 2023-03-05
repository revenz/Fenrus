namespace Fenrus.Models;

/// <summary>
/// User
/// </summary>
public class User: IModal
{
    /// <summary>
    /// Gets or sets Uid of the user
    /// </summary>
    [LiteDB.BsonId] 
    public Guid Uid { get; set; }

    /// <summary>
    /// Alias for username
    /// </summary>
    public string Name
    {
        get => Username;
        set => Username = value;
    }

    /// <summary>
    /// Gets or sets the username
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Gets or sets the email address for the user
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the users password
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets if the user is an admin
    /// </summary>
    public bool IsAdmin { get; set; }
}