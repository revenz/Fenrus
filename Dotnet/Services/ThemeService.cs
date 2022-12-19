using System.Text.RegularExpressions;
using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Service for themes
/// </summary>
public class ThemeService
{
    /// <summary>
    /// Gets a theme by its name
    /// </summary>
    /// <param name="name">the name of the theme</param>
    /// <returns>the them</returns>
    public Theme GetTheme(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
            return new Theme();
        
        name = Regex.Replace(name, "[^a-z0-9_]", string.Empty, RegexOptions.IgnoreCase).Trim(); //  make the name safe
        if(name == string.Empty)
            return new Theme();
        
        var theme = new Theme();
        theme.Name = name;
        theme.Directory = Path.Combine("..", "wwwroot", "themes", name);
        theme.DirectoryName = name;
        var file = Path.Combine(theme.Directory, "theme.json");
        if (File.Exists(file) == false)
        {
            Logger.ILog("NO THEME FILE FOUND IN: " + file);
            return theme; // basic theme nothing more to load
        }

        // more complex theme, with theme file        
        var json = File.ReadLines(file);

        var deserialized = JsonSerializer.Deserialize<Theme>(file);
        if (deserialized == null)
            return theme;
        theme = deserialized;
        
        // if(!theme.Templates)
        //     theme.Templates = {
        //     Group: '',
        //     App: '',
        //     Link: ''
        // };
        
        return theme;
    }
    
    
    /// <summary>
    /// Gets a list of themes in the system
    /// </summary>
    /// <returns>a list of themes</returns>
    public List<string> GetThemes()
    {
        List<string> themes = new();
        foreach (var dir in new DirectoryInfo(DirectoryHelper.GetWwwRootDirectory()).GetDirectories())
        {
            themes.Add(dir.Name);
        }
        return themes;
    }
}