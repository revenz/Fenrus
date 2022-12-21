using Fenrus.Services;

namespace Fenrus.Pages;

/// <summary>
/// Dashboards Page
/// </summary>
public partial class Dashboards: CommonPage<Models.Dashboard>
{
    public List<Models.Dashboard> Items { get; set; } = new();

    protected override async Task PostGotUser()
    {
        Items = Settings.Dashboards;
    }
}