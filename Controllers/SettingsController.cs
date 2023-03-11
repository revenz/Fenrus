using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for user settings
/// </summary>
[Authorize]
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
        var service = new GroupService();
        var group = service.GetByUid(groupUid);
        if (group == null)
            return NotFound();

        var item = group.Items.FirstOrDefault(x => x.Uid == itemUid);
        if (item == null)
            return NotFound();
        if (item.Size == size)
            return Content(string.Empty); // nothing to do
        
        item.Size = size;
        service.Update(group);
        return Content(string.Empty);
    }

    /// <summary>
    /// Deletes a item from a group
    /// </summary>
    /// <param name="groupUid">the UID of the group</param>
    /// <param name="itemUid">the UID of the item</param>
    [HttpDelete("settings/groups/{groupUid}/delete/{itemUid}")]
    public IActionResult DeleteItem([FromRoute] Guid groupUid, [FromRoute] Guid itemUid)
    {
        var service = new GroupService();
        var group = service.GetByUid(groupUid);
        if (group == null)
            return NotFound();

        var item = group.Items.FirstOrDefault(x => x.Uid == itemUid);
        if (item == null)
            return Content(string.Empty);
        
        group.Items.Remove(item);
        service.Update(group);
        return Content(string.Empty);
    }


    /// <summary>
    /// Updates a user setting
    /// </summary>
    /// <param name="setting">The setting being updated</param>
    /// <param name="value">the new value</param>
    /// <returns>result from the update</returns>
    [HttpPost("settings/update-setting/{setting}/{value}")]
    public async Task<IActionResult> UpdateSetting([FromRoute] string setting, [FromRoute] string value)
    {
        var settings = GetUserSettings();
        bool reload = false;
        bool changed = false;
        switch (setting)
        {
            case nameof(settings.Language):
                if (value != null && value != settings.Language && Regex.IsMatch(value, "^[a-z]+$"))
                {
                    changed = true;
                    settings.Language = value;
                    reload = true;
                }
                break;
        }
        
        if(changed)
            settings.Save();
        
        return Json(new
        {
            reload
        });
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
        var service = new DashboardService();
        var dashboard = service.GetByUid(uid);
        if (dashboard?.UserUid != GetUserUid())
        {
            Response.StatusCode = 500;
            return Json(new
            {
                Error = "Invalid dashboard"
            });
        }
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
            case nameof(dashboard.LinkTarget):
                dashboard.LinkTarget = value;
                break;
            case nameof(dashboard.ShowSearch):
                dashboard.ShowSearch = bool.Parse(value);
                break;
            case nameof(dashboard.ShowStatusIndicators):
                dashboard.ShowStatusIndicators = bool.Parse(value);
                break;
            case nameof(dashboard.ShowGroupTitles):
                dashboard.ShowGroupTitles = bool.Parse(value);
                break;
            default:
                return NotFound();
        }
        service.Update(dashboard);
        
        return Json(new
        {
            reload
        });
    }


    /// <summary>
    /// Removes a group from a dashboard
    /// </summary>
    /// <param name="uid">The UID of the dashboard to update the setting for</param>
    /// <param name="groupUid">The UID of the group being removed</param>
    [HttpPost("settings/dashboard/{uid}/remove-group/{groupUid}")]
    public void RemoveGroup([FromRoute] Guid uid, [FromRoute] Guid groupUid)
    {
        var service = new DashboardService();
        var dashboard = service.GetByUid(uid);;
        if (dashboard?.UserUid != GetUserUid())
            return;
        dashboard.GroupUids = dashboard.GroupUids.Where(x => x != groupUid).ToList();
        service.Update(dashboard); 
    }

    /// <summary>
    /// Moves a group inside a dashboard
    /// </summary>
    /// <param name="uid">The UID of the dashboard to update the setting for</param>
    /// <param name="groupUid">The UID of the group moving up</param>
    /// <param name="up">true if moving the group up, otherwise false</param>
    [HttpPost("settings/dashboard/{uid}/move-group/{groupUid}/{up}")]
    public void MoveGroup([FromRoute] Guid uid, [FromRoute] Guid groupUid, [FromRoute] bool up)
    {
        var service = new DashboardService();
        var dashboard = service.GetByUid(uid);;
        if (dashboard?.UserUid != GetUserUid())
            return;
        var index = dashboard.GroupUids.IndexOf(groupUid);
        if (index < 0)
            return;
        if (up && index < 1)
            return;
        if (up == false && index >= dashboard.GroupUids.Count - 1)
            return;
        int dest = index + (up ? -1 : 1);
        dashboard.GroupUids[index] = dashboard.GroupUids[dest];
        dashboard.GroupUids[dest] = groupUid;
        dashboard.GroupUids = dashboard.GroupUids.Distinct().ToList();
        service.Update(dashboard); 
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