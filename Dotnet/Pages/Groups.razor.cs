namespace Fenrus.Pages;

public partial class Groups
{
    public List<Models.Group> Items { get; set; } = new();

    protected override void OnInitialized()
    {
        Items = DemoHelper.GetDemoUserSettings().Groups;
    }

    private async Task Move(Models.Group item, bool up)
    {
        
    }

    private async Task Remove(Models.Group item)
    {
        
    }

    private async Task Add()
    {
        
    }
}