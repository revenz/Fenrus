using Fenrus.Models;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Panel for the users theme settings
/// </summary>
public partial class PanelThemeSettings : ComponentBase
{
    /// <summary>
    /// Gets or sets the theme the user is using
    /// </summary>
    [Parameter]
    public Theme Theme { get; set; }   
    
    /// <summary>
    /// Gets or sets the user settings
    /// </summary>
    [Parameter]
    public UserSettings Settings { get; set; }
    
    /// <summary>
    /// Gets or sets if the current user is a guest or logged in user
    /// </summary>
    [Parameter]
    public bool IsGuest { get; set; }

    private bool GetThemeValue(ThemeSetting setting, out object? value)
    {
        if (Settings.ThemeSettings == null || Settings.ThemeSettings.ContainsKey(Theme.Name) == false)
        {
            value = null;
            return false;
        }

        var dict = Settings.ThemeSettings[Theme.Name];
        return dict.TryGetValue(setting.Name, out value);
    }
    
    string GetStringValue(ThemeSetting setting)
    {
        if (GetThemeValue(setting, out object value) && value != null)
            return value.ToString();
        return setting.DefaultValue?.ToString() ?? string.Empty;
    }

    bool GetBoolValue(ThemeSetting setting)
    {
        if (GetThemeValue(setting, out object value) && value is bool b)
            return b;
        return setting.DefaultValue as bool? == true;
    }

    int GetIntValue(ThemeSetting setting)
    {
        if (GetThemeValue(setting, out object value) && value is int i)
            return i;
        if (setting.DefaultValue == null)
            return 0;
        if (int.TryParse(setting.DefaultValue.ToString(), out int i2))
            return i2;
        return 0;
    }
}