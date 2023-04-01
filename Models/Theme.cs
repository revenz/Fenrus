using System.Text.Json.Serialization;
using Fenrus.Models.UiModels;

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
    /// Gets or sets a size to force apps to, can be null
    /// </summary>
    public ItemSize? ForcedSize { get; set; }

    /// <summary>
    /// Gets or sets any CSS files for the theme
    /// </summary>
    public List<string> Css
    {
        get => _Css;
        set
        {
            if (value == _Css)
                return; // dont call clear here, this would wipe it out
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
            if (value == _Scripts)
                return; // dont call clear here, this would wipe it out
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
    public List<ThemeSetting> Settings { get; set; }
}

/// <summary>
/// Theme property
/// </summary>
public class ThemeSetting
{
    /// <summary>
    /// Gets or sets the human readable name for this property
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the label, if not set name will be used 
    /// </summary>
    public string Label { get; set; }
    
    /// <summary>
    /// Gets or sets the type of property
    /// </summary>
    public AppPropertyType Type { get; set; }

    /// <summary>
    /// Gets or sets the default value
    /// </summary>
    [JsonPropertyName("default")]
    public object DefaultValue { get; set; }
    
    /// <summary>
    /// Gets or sets list options for a Select input
    /// </summary>
    public List<ListOption> Options { get; set; }

    /// <summary>
    /// Gets or sets the minimum
    /// </summary>
    public int? Minimum { get; set; }

    /// <summary>
    /// Gets or sets the maximum
    /// </summary>
    public int? Maximum { get; set; }
}