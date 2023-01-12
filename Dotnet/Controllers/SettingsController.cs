using Humanizer;
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
    /// Updates a setting
    /// </summary>
    /// <returns>result from the update</returns>
    [HttpPost("settings/update-setting/{setting}/{value}")]
    public IActionResult UpdateSetting([FromRoute] string setting, [FromRoute] string value)
    {
        bool reload = false;
        var settings = GetUserSettings();
        switch (setting)
        {
            case "LinkTarget":
                settings.LinkTarget = value;
                settings.Save();
                break;
            case "Theme":
                if (settings.Theme != value || settings.Dashboards.Any(x => string.IsNullOrEmpty(x.Theme) == false && x.Theme != value))
                {
                    foreach (var db in settings.Dashboards)
                        db.Theme = string.Empty; // switches the dashboard to the default theme..
                    reload = true;
                    settings.Theme = value;
                    settings.Save();
                }
                break;
            case "ShowGroupTitles":
                settings.ShowGroupTitles = value?.ToLower() == "true";
                settings.Save();
                break;
            case "ShowStatusIndicators":
                settings.ShowStatusIndicators = value?.ToLower() == "true";
                settings.Save();
                break;
            default:
                return NotFound();
        }

        return Json(new
        {
            Reload = reload,
            settings.Theme,
            settings.LinkTarget,
            settings.ShowGroupTitles,
            settings.ShowStatusIndicators
        });
    }
}