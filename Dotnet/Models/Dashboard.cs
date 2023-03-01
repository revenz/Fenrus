namespace Fenrus.Models;

/// <summary>
/// Dashboard
/// </summary>
public class Dashboard: IModal
{
    /// <summary>
    /// Gets or sets the Uid of the dashboard
    /// </summary>
    [LiteDB.BsonId]
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
    /// Gets or sets the background for the dashboard
    /// </summary>
    public string Background { get; set; }
    
    /// <summary>
    /// Gets or sets the background color for the dashboard
    /// </summary>
    public string BackgroundColor { get; set; }
    
    /// <summary>
    /// Gets or sets the theme used by the dashboard
    /// </summary>
    public string Theme { get; set; }

    /// <summary>
    /// Gets or sets the dashboards accent color, as #rrggbb, for example '#ff0090'
    /// </summary>
    public string AccentColor { get; set; }

    /// <summary>
    /// Gets or sets the target to open dashboard links
    /// </summary>
    public string LinkTarget { get; set; }

    /// <summary>
    /// Gets or sets if group titles should be shown
    /// </summary>
    public bool ShowGroupTitles { get; set; }

    /// <summary>
    /// Gets or sets if status indicators should be shown
    /// </summary>
    public bool ShowStatusIndicators { get; set; }

    /// <summary>
    /// Gets or sets if search should be shown
    /// </summary>
    public bool ShowSearch { get; set; }


    private List<Group> _Groups = new();

    /// <summary>
    /// Gets or sets the Groups in the dashboard
    /// </summary>
    public List<Group> Groups
    {
        get => _Groups;
        set
        {
            if (value == _Groups)
                return; // same object
            _Groups.Clear();
            if(value?.Any() == true)
                _Groups.AddRange(value);
        }
    }
}