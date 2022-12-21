namespace Fenrus.Pages;

public partial class Groups: CommonPage<Models.Group>
{
    public List<Models.Group> Items { get; set; } = new();

    protected override async Task PostGotUser()
    {
        Items = Settings.Groups;
    }
}