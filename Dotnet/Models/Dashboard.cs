namespace Fenrus.Models;

/// <summary>
/// Dashboard
/// </summary>
public class Dashboard: IModal
{
    /// <summary>
    /// Gets or sets the Uid of the dashboard
    /// </summary>
    public Guid Uid { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the dashboard
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets if the dashboard is enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the background image for the dashboard
    /// </summary>
    public string BackgroundImage { get; set; }
    
    /// <summary>
    /// Gets or sets the theme used by the dashboard
    /// </summary>
    public string Theme { get; set; }

    /// <summary>
    /// Gets or sets the dashboards accent color, as #rrggbb, for example '#ff0090'
    /// </summary>
    public string AccentColor { get; set; }


    private List<Group> _Groups = new();

    /// <summary>
    /// Gets or sets the Groups in the dashboard
    /// </summary>
    public List<Group> Groups
    {
        get => _Groups;
        set
        {
            _Groups.Clear();
            if(value?.Any() == true)
                _Groups.AddRange(value);
        }
    }
}