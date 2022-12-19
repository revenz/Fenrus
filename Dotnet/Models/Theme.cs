namespace Fenrus.Models;

/// <summary>
/// Theme
/// </summary>
public class Theme
{

    public readonly List<string> _Css = new();
    public readonly List<string> _Scripts = new();
    
    /// <summary>
    /// Gets or sets the themes name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets any CSS files for the theme
    /// </summary>
    public List<string> Css
    {
        get => _Css;
        set
        {
            _Css.Clear();
            if (value?.Any() == true)
                _Css.AddRange(value);
        }
    }

    /// <summary>
    /// Gets or sets any script files for the theme
    /// </summary>
    public List<string> Scripts
    {
        get => _Scripts;
        set
        {
            _Scripts.Clear();
            if (value?.Any() == true)
                _Scripts.AddRange(value);
        }
    }

    //public object Templates {get;set;}

    /// <summary>
    /// Gets or sets the full  directory of the theme
    /// </summary>
    public string Directory { get; set; }

    /// <summary>
    /// Gets or sets the directory name (just the directory name) of the theme
    /// </summary>
    public object DirectoryName { get; set; }

    /// <summary>
    /// Gets or sets any theme settings
    /// </summary>
    public object Settings { get; set; }

}