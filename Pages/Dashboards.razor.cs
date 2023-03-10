using Fenrus.Components;

namespace Fenrus.Pages;

/// <summary>
/// Dashboards Page
/// </summary>
public partial class Dashboards: CommonPage<Models.Dashboard>
{
    private string lblTitle, lblDescription;
    
    /// <summary>
    /// Gets or sets the table
    /// </summary>
    private FenrusTable<Models.Dashboard> Table { get; set; }
    
    /// <summary>
    /// Gets or sets the dashboards bound to the table
    /// </summary>
    public List<Models.Dashboard> Items { get; set; } = new();

    /// <summary>
    /// Called after the user has been loaded
    /// </summary>
    protected override Task PostGotUser()
    {
        lblTitle = Translator.Instant("Pages.Dashboards.Title");
        lblDescription = Translator.Instant("Pages.Dashboards.Labels.PageDescription");
        var service = new DashboardService();
        Items = (IsSystem ? service.GetAllSystem() : service.GetAllForUser(UserUid)).OrderBy(x => x.Name).ToList();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets enabled state of dashboard
    /// </summary>
    /// <param name="dashboard">the dashboard to enable</param>
    /// <param name="enabled">the enabled state of the dashboard</param>
    private void Enable(Models.Dashboard dashboard, bool enabled)
    {
        dashboard.Enabled = enabled;
        new DashboardService().Update(dashboard);
    }

    /// <summary>
    /// Sets "Default" for a dashboard
    /// </summary>
    /// <param name="engine">the dashboard to update</param>
    /// <param name="isDefault">the default state</param>
    private void SetDefault(Models.Dashboard engine, bool isDefault)
    {
        var service = new DashboardService();
        if (isDefault) 
        {
            // only clear other defaults when setting this one as a default
            foreach (var item in this.Items)
            {
                if (item.IsDefault && item.Uid != engine.Uid)
                {
                    // remove the default
                    item.IsDefault = false;
                    service.Update(item);
                }
            }
        }

        if (engine.IsDefault != isDefault)
        {
            engine.IsDefault = isDefault;
            service.Update(engine);
        }
        Table.TriggerStateHasChanged();
    }
    
    protected override bool DoDelete(Models.Dashboard item)
    {
        Items.RemoveAll(x => x.Uid == item.Uid);
        new DashboardService().Delete(item.Uid);
        Table.SetData(Items);
        return true;
    }
}