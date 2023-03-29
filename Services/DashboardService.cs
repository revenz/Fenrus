using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Service for Dashboards
/// </summary>
public class DashboardService
{
    /// <summary>
    /// Gets a dashboard by its UID
    /// </summary>
    /// <param name="uid">the UID of dashboard</param>
    /// <returns>the dashboard</returns>
    public Dashboard GetByUid(Guid uid) => DbHelper.GetByUid<Dashboard>(uid);
    
    /// <summary>
    /// Gets all dashboards
    /// </summary>
    /// <returns>all dashboards</returns>
    public List<Dashboard> GetAll()
        => DbHelper.GetAll<Dashboard>();

    /// <summary>
    /// Gets all dashboards for a user
    /// </summary>
    /// <param name="uid">The UID of the user</param>
    /// <returns>all dashboards</returns>
    public List<Dashboard> GetAllForUser(Guid uid)
        => DbHelper.GetAllForUser<Dashboard>(uid);
    
    /// <summary>
    /// Gets all system dashboards for a user
    /// </summary>
    /// <returns>all system dashboards</returns>
    public List<Dashboard> GetAllSystem()
        => DbHelper.GetAllForUser<Dashboard>(Guid.Empty);
    
    /// <summary>
    /// Gets the guest dashboard 
    /// </summary>
    /// <returns>the guest dashboard</returns>
    public Dashboard GetGuestDashboard()
    {
        var dashboard = DbHelper.GetByUid<Dashboard>(Globals.GuestDashbardUid);
        if (dashboard != null)
            return dashboard;
        dashboard = new();
        dashboard.Uid = Globals.GuestDashbardUid;
        dashboard.Name = "Guest Dashboard";
        dashboard.Enabled = true;
        dashboard.Theme = "Default";
        dashboard.AccentColor = "#ff0090";
        dashboard.BackgroundColor = "#009099";
        DbHelper.Insert(dashboard);
        return dashboard;
    }

    /// <summary>
    /// Saves the dashboard
    /// </summary>
    /// <param name="dashboard">the dashboard being updated</param>
    public void Update(Dashboard dashboard)
    {
        if (dashboard.Uid == Guid.Empty)
        {
            dashboard.Uid = Guid.NewGuid();
            DbHelper.Insert(dashboard);
        }
        else
        {
            DbHelper.Update(dashboard);
        }
    }
    
    /// <summary>
    /// Adds a new dashboard
    /// </summary>
    /// <param name="dashboard">the new dashboard being added</param>
    public void Add(Dashboard dashboard)
    {
        if(dashboard.Uid == Guid.Empty)
            dashboard.Uid = Guid.NewGuid();
        DbHelper.Insert(dashboard);
    }
    
    /// <summary>
    /// Deletes a dashboard
    /// </summary>
    /// <param name="uid">the UID of the dashboard</param>
    public void Delete(Guid uid)
        => DbHelper.Delete<Dashboard>(uid);

    /// <summary>
    /// Gets the first dashboards accent color for a user
    /// </summary>
    /// <param name="userUid">the users UID</param>
    /// <returns>the accent color for this user</returns>
    public string GetAccentColorForUser(Guid userUid)
    {
        var db = DbHelper.GetDb();
        var collection = db.GetCollection<Dashboard>(nameof(Dashboard));
        return collection.Query().Where(x => x.UserUid == userUid && x.Enabled && x.AccentColor != null)
                   .OrderBy(x => x.IsDefault ? 0 : 1)
                   .FirstOrDefault()
                   ?.AccentColor ??
               Globals.DefaultAccentColor;
    }
}