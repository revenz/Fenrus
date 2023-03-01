using System.Text.RegularExpressions;
using Jint.Runtime.Debugger;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for user settings
/// </summary>
public class SettingsController : BaseController
{
    /// <summary>
    /// Resizes a item in a group
    /// </summary>
    /// <param name="groupUid">the UID of the group</param>
    /// <param name="itemUid">the UID of the item</param>
    /// <param name="size">the new size</param>
    [HttpPost("settings/groups/{groupUid}/resize/{itemUid}/{size}")]
    public IActionResult ResizeItem([FromRoute] Guid groupUid, [FromRoute] Guid itemUid, [FromRoute] ItemSize size)
    {
        var settings = GetUserSettings();
        if (settings == null)
            throw new UnauthorizedAccessException();

        var group = settings.Groups.FirstOrDefault(x => x.Uid == groupUid);
        if (group == null)
            return NotFound();

        var item = group.Items.FirstOrDefault(x => x.Uid == itemUid);
        if (item == null)
            return NotFound();
        if (item.Size == size)
            return Content(""); // nothing to do
        
        item.Size = size;
        settings.Save();
        return Content("");
    }

    /// <summary>
    /// Deletes a item from a group
    /// </summary>
    /// <param name="groupUid">the UID of the group</param>
    /// <param name="itemUid">the UID of the item</param>
    [HttpDelete("settings/groups/{groupUid}/delete/{itemUid}")]
    public IActionResult DeleteItem([FromRoute] Guid groupUid, [FromRoute] Guid itemUid)
    {
        var settings = GetUserSettings();
        if (settings == null)
            throw new UnauthorizedAccessException();

        var group = settings.Groups.FirstOrDefault(x => x.Uid == groupUid);
        if (group == null)
            return NotFound();

        var item = group.Items.FirstOrDefault(x => x.Uid == itemUid);
        if (item == null)
            return Content("");
        
        group.Items.Remove(item);
        settings.Save();
        return Content("");
    }

    /// <summary>
    /// Updates a dashboard setting
    /// </summary>
    /// <param name="uid">The UID of the dashboard to update the setting for</param>
    /// <param name="setting">The setting being updated</param>
    /// <param name="value">the new value</param>
    /// <returns>result from the update</returns>
    [HttpPost("settings/dashboard/{uid}/update-setting/{setting}/{value}")]
    public async Task<IActionResult> UpdateDashboardSetting([FromRoute] Guid uid, [FromRoute] string setting, [FromRoute] string value)
    {
        if (uid == Guid.Empty)
        {
            Response.StatusCode = 500;
            return Json(new
            {
                Error = "Invalid Dashboard"
            });
        }

        if (string.IsNullOrWhiteSpace(setting))
        {
            Response.StatusCode = 500;
            return Json(new
            {
                Error = "Invalid setting"
            });
        }
        bool reload = false;
        var settings = GetUserSettings();
        var dashboard = settings.Dashboards?.FirstOrDefault(x => x.Uid == uid);
        switch (setting)
        {
            case nameof(dashboard.AccentColor):
                dashboard.AccentColor = value;
                break;
            case nameof(dashboard.BackgroundColor):
                dashboard.BackgroundColor = value;
                break;
            case nameof(dashboard.Background):
                dashboard.Background = value;
                break;
            case nameof(dashboard.BackgroundImage):
                {
                    using var ms = new MemoryStream(2048);
                    await Request.Body.CopyToAsync(ms);
                    byte[] data = ms.ToArray();
                    dashboard.BackgroundImage = ImageHelper.SaveImage(data, value, uid: dashboard.Uid);
                }
                break;
            case nameof(dashboard.Theme):
                dashboard.Theme = value;
                reload = true;
                break;
            default:
                return NotFound();
        }
        settings.Save();
        
        return Json(new
        {
            reload
        });
    }

    /// <summary>
    /// Updates a theme setting
    /// </summary>
    /// <param name="theme">The theme the setting is being updated for</param>
    /// <param name="setting">The setting being updated</param>
    /// <param name="value">the new value</param>
    /// <returns>result from the update</returns>
    [HttpPost("settings/theme/{theme}/update-setting/{setting}/{value}")]
    public IActionResult UpdateThemeSetting([FromRoute] string theme, [FromRoute] string setting, [FromRoute] string value)
    {
        if (string.IsNullOrWhiteSpace(theme))
        {
            Response.StatusCode = 500;
            return Json(new
            {
                Error = "Invalid theme"
            });
        }

        if (string.IsNullOrWhiteSpace(setting))
        {
            Response.StatusCode = 500;
            return Json(new
            {
                Error = "Invalid setting"
            });
        }
        var settings = GetUserSettings();
        settings.ThemeSettings ??= new();
        if(settings.ThemeSettings.ContainsKey(theme) == false)
            settings.ThemeSettings.Add(theme, new Dictionary<string, object?>());
        else if (settings.ThemeSettings[theme] == null)
            settings.ThemeSettings[theme] = new();
        var dict = settings.ThemeSettings[theme];

        object oValue = value;
        if (string.IsNullOrWhiteSpace(value) == false)
        {
            if (value.ToLower() == "true")
                oValue = true;
            else if (value.ToLower() == "false")
                oValue = false;
            else if (Regex.IsMatch(value, @"^[\d]+$"))
                oValue = int.Parse(value);
        }

        if (dict.ContainsKey(setting))
            dict[setting] = oValue;
        else
            dict.Add(setting, oValue);
        settings.Save();
        
        return Json(new
        {
        });
    }
}