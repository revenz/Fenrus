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
    /// Gets or sets the users password
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets if the user is an admin
    /// </summary>
    public bool IsAdmin { get; set; }
    
    
    private List<object> _Items = new();

    /// <summary>
    /// Gets or sets the items in the group
    /// </summary>
    public List<object> Items
    {
        get => _Items;
        set
        {
            if (value == _Items)
                return; // dont call clear here, this would wipe it out
            _Items.Clear();
            if(value?.Any() == true)
                _Items.AddRange(value);
        }
    }
}