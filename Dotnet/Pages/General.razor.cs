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
        if (string.IsNullOrEmpty(Background))
            return;
        // data:image/jpeg;base64,
        if (Background.StartsWith("data:image/") == false)
            return; // not valid base64 image
        string b64 = Background.Substring("data:image/".Length);
        string extension = b64.Substring(0, b64.IndexOf(";")).ToLower();
        if (extension == "jpeg")
            extension = "jpg";
        b64 = b64.Substring(b64.IndexOf(",") + 1);
        var data = Convert.FromBase64String(b64);
    }
}