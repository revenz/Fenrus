using Fenrus.Services;

namespace Fenrus.Pages;

/// <summary>
/// Dashboards Page
/// </summary>
public partial class Dashboards
{
    public List<Models.Dashboard> Items { get; set; } = new();

    protected override void OnInitialized()
    {
        Items = DemoHelper.GetDemoUserSettings().Dashboards;
    }

    private async Task Move(Models.Dashboard item, bool up)
    {
        
    }

    private async Task Remove(Models.Dashboard item)
    {
        
    }

    private async Task Add()
    {
        
    }
}