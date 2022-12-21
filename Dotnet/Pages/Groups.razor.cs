namespace Fenrus.Pages;

public partial class Groups: CommonPage<Models.Group>
{
    public List<Models.Group> Items { get; set; } = new();

    protected override async Task PostGotUser()
    {
        if (IsSystem)
            Items = DbHelper.GetAll<Models.Group>();
        else
            Items = Settings.Groups;
    }
}