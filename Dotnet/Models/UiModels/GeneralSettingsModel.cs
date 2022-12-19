namespace Fenrus.Models.UiModels;

/// <summary>
/// UI Model for general settings
/// </summary>
public class GeneralSettingsModel
{
    public string Theme { get; set; }
    public string LinkTarget { get; set; }
    public string AccentColor { get; set; }
    public bool GroupTitles { get; set; }
    public bool ShowIndicators { get; set; }
}