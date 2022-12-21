using Fenrus.Models;
using Fenrus.Models.UiModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Fenrus.Pages;

/// <summary>
/// General user settings page
/// </summary>
[Authorize]
public partial class General : UserPage
{
    GeneralSettingsModel Model { get; set; } = new();

    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    private string Background { get; set; }

    protected override async Task PostGotUser()
    {
        this.Model = new()
        {
            Theme = Settings.Theme,
            AccentColor = Settings.AccentColor,
            GroupTitles = Settings.ShowGroupTitles,
            LinkTarget = Settings.LinkTarget,
            ShowIndicators = Settings.ShowStatusIndicators
        };
    }

    private async Task Save()
    {
        SaveBackground();
        Settings.AccentColor = Model.AccentColor;
        Settings.LinkTarget = Model.LinkTarget;
        Settings.Theme = Model.Theme;
        Settings.ShowGroupTitles = Model.GroupTitles;
        Settings.ShowStatusIndicators = Model.ShowIndicators;
        new Services.UserSettingsService().Save(Settings);
    }

    void SaveBackground()
    {
        var image = ImageHelper.ImageFromBase64(Background);
        if (image.Data?.Any() != true)
            return;
    }
}