using Fenrus.Components.Dialogs;
using JavaScriptEngineSwitcher.Core.Resources;

namespace Fenrus.Pages;

/// <summary>
/// Search Engines page
/// </summary>
public partial class SearchEngines: CommonPage<Models.SearchEngine>
{
    public List<Models.SearchEngine> Items { get; set; } = new();

    protected override async Task PostGotUser()
    {
        Items = Settings.SearchEngines;
    }
}