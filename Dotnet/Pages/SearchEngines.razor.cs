namespace Fenrus.Pages;

/// <summary>
/// Search Engines page
/// </summary>
public partial class SearchEngines
{
    public List<Models.SearchEngine> Items { get; set; } = new();

    protected override void OnInitialized()
    {
        Items = DemoHelper.GetDemoUserSettings().SearchEngines;
    }

    private async Task Remove(Models.SearchEngine item)
    {
        
    }

    private async Task Add()
    {
        
    }
}