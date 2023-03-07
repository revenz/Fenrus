using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Service for Dashboards
/// </summary>
public class DashboardService
{
    /// <summary>
    /// Gets all dashboards
    /// </summary>
    /// <returns>all dashboards</returns>
    public List<Dashboard> GetAll()
        => DbHelper.GetAll<Dashboard>();

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
}