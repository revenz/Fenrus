namespace Fenrus.Pages;

public partial class Groups: CommonPage<Models.Group>
{
    public List<Models.Group> Items { get; set; } = new();

    protected override void OnInitialized()
    {
        Items = DemoHelper.GetDemoUserSettings().Groups;
    }
}