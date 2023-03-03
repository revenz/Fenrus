using Fenrus.Components.SideEditors;
using Fenrus.Pages;
using Fenrus.Services;
using SearchEngine = Fenrus.Models.SearchEngine;

namespace Fenrus.Components;

/// <summary>
/// Search Engines page
/// </summary>
public partial class PageSearchEngines: CommonPage<Models.SearchEngine>
{
    public List<Models.SearchEngine> Items { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the table
    /// </summary>
    private FenrusTable<SearchEngine> Table { get; set; }

    private string Title => IsSystem ? "System Search Engines" : "Search Engines";

    private string Description => IsSystem
        ? "This page lets you configure system wide search engines that will be available to all users and guests on the system."
        : @"This page lets you configure search engines that can be used on the home screen.

If you do not configure any and have no system search engines configured, the search box will not appear on the home screen.

To switch to an non-default search engine on the home screen type in the search engine Shortcut followed by a space.";

    protected override async Task PostGotUser()
    {
        if (IsSystem)
            Items = DbHelper.GetAll<Models.SearchEngine>().OrderBy(x => x.Name).ToList();
        else
            Items = Settings.SearchEngines.OrderBy(x => x.Name).ToList();
    }

    protected override bool DoDelete(Models.SearchEngine item)
    {
        Items.RemoveAll(x => x.Uid == item.Uid);
        if (IsSystem)
        {
            new SearchEngineService().Delete(item.Uid);
        }
        else
        {
            Settings.SearchEngines.RemoveAll(x => x.Uid == item.Uid);
            Settings.Save();
        }
        Table.TriggerStateHasChanged();
        StateHasChanged();
        return true;
    }

    private async Task Edit(SearchEngine engine)
    {
        var result = await Popup.OpenEditor<SearchEngineEditor, SearchEngine>(engine, new ()
        {
            { nameof(SearchEngineEditor.IsSystem), IsSystem },
            { nameof(SearchEngineEditor.Settings), Settings }
        });
        if (result.Success == false)
            return;
        StateHasChanged();
        Table.TriggerStateHasChanged();
    }
    private async Task Add()
    {
        var result = await Popup.OpenEditor<SearchEngineEditor, SearchEngine>(null, new ()
        {
            { nameof(SearchEngineEditor.IsSystem), IsSystem },
            { nameof(SearchEngineEditor.Settings), Settings }
        });
        if (result.Success == false)
            return;
        Items.Add(result.Data);
        Table.SetData(Items);
    }
}