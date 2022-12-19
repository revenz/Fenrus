using Fenrus.Models.UiModels;

namespace Fenrus.Pages;

/// <summary>
/// General user settings page
/// </summary>
public partial class General
{
    GeneralSettingsModel Model { get; set; } = new();
    
    private async Task Save()
    {
        Console.WriteLine("saving!");
    }
}