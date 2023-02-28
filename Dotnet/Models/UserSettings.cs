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
    /// Gets or sets the users name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the revision of the saved settings
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// Gets or sets the target to open dashboard links
    /// </summary>
    public string LinkTarget { get; set; }

    /// <summary>
    /// Gets or sets the users theme
    /// </summary>
    public string Theme { get; set; }

    /// <summary>
    /// Gets or sets if menus should be collapsed
    /// </summary>
    public bool CollapseMenu { get; set; }

    /// <summary>
    /// Gets or sets the accent color
    /// </summary>
    public string AccentColor { get; set; }

    /// <summary>
    /// Gets or sets if group titles should be shown
    /// </summary>
    public bool ShowGroupTitles { get; set; }

    /// <summary>
    /// Gets or sets the default background image for dashboards
    /// </summary>
    public string BackgroundImage { get; set; }

    /// <summary>
    /// Gets or sets the default background for dashboards
    /// </summary>
    public string Background { get; set; }

    /// <summary>
    /// Gets or sets the default background color for dashboards
    /// </summary>
    public string BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets if search should be shown
    /// </summary>
    public bool ShowSearch { get; set; }

    /// <summary>
    /// Gets or sets if status indicators should be shown
    /// </summary>
    public bool ShowStatusIndicators { get; set; }

    /// <summary>
    /// Gets or sets the users theme settings
    /// </summary>
    public Dictionary<string, Dictionary<string, object?>> ThemeSettings { get; set; }

    private readonly List<DockerServer> _Docker = new ();
    private readonly List<Group> _Groups = new ();
    private readonly List<Dashboard> _Dashboards = new ();
    private readonly List<SearchEngine> _SearchEngines = new ();

    /// <summary>
    /// Gets or sets the configured docker instances
    /// </summary>
    public List<DockerServer> Docker 
    {
        get => _Docker;
        set
        {
            if (value == _Docker)
                return; // dont call clear here, this would wipe it out
            _Docker.Clear();
            if (value != null)
                _Docker.AddRange(value);
        }
    }

    /// <summary>
    /// Gets or sets groups for the user
    /// </summary>
    public List<Group> Groups 
    {
        get => _Groups;
        set
        {
            if (value == _Groups)
                return; // dont call clear here, this would wipe it out
            _Groups.Clear();
            if (value != null)
                _Groups.AddRange(value);
        }
    }

    /// <summary>
    /// Gets or sets the users dashboards
    /// </summary>
    public List<Dashboard> Dashboards
    {
        get => _Dashboards;
        set
        {
            if (value == _Dashboards)
                return; // dont call clear here, this would wipe it out
            _Dashboards.Clear();
            if(value != null)
                _Dashboards.AddRange(value);
        }
    }
    
    /// <summary>
    /// Gets or sets the users search engines
    /// </summary>
    public List<SearchEngine> SearchEngines
    {
        get => _SearchEngines;
        set
        {
            if (value == _SearchEngines)
                return; // dont call clear here, this would wipe it out
            _SearchEngines.Clear();
            if (value != null)
                _SearchEngines.AddRange(value);
        }
    }

    /// <summary>
    /// Saves the user settings
    /// </summary>
    public void Save()
        => new UserSettingsService().Save(this);
}