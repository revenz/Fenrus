using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Pages;

/// <summary>
/// Search Engines page
/// </summary>
public partial class SearchEngines: CommonPage<Models.SearchEngine>
{
    public List<Models.SearchEngine> Items { get; set; } = new();

    private string Title => IsSystem ? "System Search Engines" : "Search Engines";

    private string Description => IsSystem
        ? "This page lets you configure system wide search engines that will be available to all users and guests on the system."
        : @"This page lets you configure search engines that can be used on the home screen.

If you do not configure any and have no system search engines configured, the search box will not appear on the home screen.

To switch to an non-default search engine on the home screen type in the search engine Shortcut followed by a space.";

    protected override async Task PostGotUser()
    {
        if (IsSystem)
            Items = DbHelper.GetAll<Models.SearchEngine>();
        else
            Items = Settings.SearchEngines;
    }

    protected override bool DoDelete(Models.SearchEngine item)
    {
        if (IsSystem)
        {
            DbHelper.Delete<Models.SearchEngine>(item.Uid);
        }
        else
        {
            Settings.SearchEngines.RemoveAll(x => x.Uid == item.Uid);
            Items.RemoveAll(x => x.Uid == item.Uid);
            Settings.Save();
        }
        StateHasChanged();
        return true;
    }
}