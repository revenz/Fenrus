namespace Fenrus.Models.UiModels;

/// <summary>
/// An option that appears it UI list
/// </summary>
public class ListOption : ListItem
{
    /// <summary>
    /// Gets or sets the display label for the option
    /// </summary>
    public string? Label { get; set; }
    
    /// <summary>
    /// Gets or sets the value for the option
    /// </summary>
    public object? Value { get; set; }
}

/// <summary>
/// A list item
/// </summary>
public abstract class ListItem
{
    /// <summary>
    /// Gets or sets the display label for the item
    /// </summary>
    public string? Label { get; set; }
}


/// <summary>
/// A list group
/// </summary>
public class ListGroup : ListItem
{
    private readonly List<ListOption> _Items = new();

    /// <summary>
    /// Gets or sets the items in the group
    /// </summary>
    public List<ListOption> Items
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