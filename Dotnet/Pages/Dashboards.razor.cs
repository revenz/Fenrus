namespace Fenrus.Pages;

/// <summary>
/// Dashboards Page
/// </summary>
public partial class Dashboards: CommonPage<Models.Dashboard>
{
    private string lblTitle, lblDescription;
    
    /// <summary>
    /// Gets or sets the dashboards bound to the table
    /// </summary>
    public List<Models.Dashboard> Items { get; set; } = new();

    /// <summary>
    /// Called after the user has been loaded
    /// </summary>
    protected override async Task PostGotUser()
    {
        lblTitle = Translater.Instant("Pages.Dashboards.Title");
        lblDescription = Translater.Instant("Pages.Dashboards.Labels.PageDescription");
        if (IsSystem)
            Items = DbHelper.GetAll<Models.Dashboard>();
        else
            Items = Settings.Dashboards;
    }

    /// <summary>
    /// Sets enabled state of dashboard
    /// </summary>
    /// <param name="dashboard">the dashboard to enable</param>
    /// <param name="enabled">the enabled state of the dashboard</param>
    private void Enable(Models.Dashboard dashboard, bool enabled)
    {
        dashboard.Enabled = enabled;
        Settings.Save();
    }
}