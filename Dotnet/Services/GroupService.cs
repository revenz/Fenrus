using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Group Service
/// </summary>
public class GroupService
{
    /// <summary>
    /// Gets the system groups
    /// </summary>
    /// <param name="enabledOnly">If only enabled groups should be fetched</param>
    /// <returns>a list of all the system groups</returns>
    public List<Group> GetSystemGroups(bool enabledOnly = false)
        => DbHelper.GetAll<Group>().Where(x => x.IsSystem && (enabledOnly == false || x.Enabled)).ToList();

    /// <summary>
    /// Gets a group by its UID
    /// </summary>
    /// <param name="uid">the UID of the object to get</param>
    /// <returns>the group</returns>
    public Group GetByUid(Guid uid)
        => DbHelper.GetByUid<Group>(uid);
}