using System.Text.RegularExpressions;

namespace Fenrus.Pages;

/// <summary>
/// The main blazor App
/// </summary>
public partial class App
{
    /// <summary>
    /// Gets the popup component
    /// </summary>
    public Components.FenrusPopup Popup { get; private set; }
    
    private string AccentRgb, AccentColor;
    /// <summary>
    /// Updates the accent color
    /// </summary>
    /// <param name="accentColor">the accent color</param>
    public void UpdateAccentColor(string accentColor)
    {
        if (Regex.IsMatch(accentColor, "^#[a-fA-F0-9]{6}") == false)
            return;
        
        AccentColor = accentColor;
        AccentRgb = int.Parse(AccentColor[1..3], System.Globalization.NumberStyles.HexNumber) + "," +
                    int.Parse(AccentColor[3..5], System.Globalization.NumberStyles.HexNumber) + "," +
                    int.Parse(AccentColor[5..7], System.Globalization.NumberStyles.HexNumber);
        StateHasChanged();
    }
}