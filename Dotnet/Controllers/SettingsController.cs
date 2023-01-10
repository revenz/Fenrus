using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for user settings
/// </summary>
[Route("settings")]
public class SettingsController : BaseController
{
    /// <summary>
    /// Resizes a item in a group
    /// </summary>
    /// <param name="groupUid">the UID of the group</param>
    /// <param name="itemUid">the UID of the item</param>
    /// <param name="size">the new size</param>
    [HttpPost("groups/{groupUid}/resize/{itemUid}/{size}")]
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
    [HttpDelete("groups/{groupUid}/delete/{itemUid}")]
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
}