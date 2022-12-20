namespace Fenrus.Models;

/// <summary>
/// Group
/// </summary>
public class Group: IModal
{
    /// <summary>
    /// Gets or sets the Uid of the group
    /// </summary>
    public Guid Uid { get; set; }

    /// <summary>
    /// Gets or sets the groups name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets if the group is enabled
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Gets or sets if the the group title should be shown
    /// </summary>
    public bool HideGroupTitle { get; set; }

    /// <summary>
    /// Gets or sets if this is a system group
    /// </summary>
    public bool IsSystem { get; set; }
    
    /// <summary>
    /// Gets or sets the groups accent colour
    /// </summary>
    public string AccentColor { get; set; }

    private readonly List<GroupItem> _Items = new();
    
    /// <summary>
    /// Gets or sets the items in the group
    /// </summary>
    public List<GroupItem> Items
    {
        get => _Items;
        set
        {
            _Items.Clear();
            if (value?.Any() == true)
                _Items.AddRange(value);
        }
    }
    
    
}