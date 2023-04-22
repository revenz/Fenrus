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

    private string lblTitle, lblDescription;

    /// <summary>
    /// Called after the user has been stored from the authentication token
    /// </summary>
    protected override Task PostGotUser()
    {
        lblTitle = Translator.Instant("Pages.SearchEngines.Title" + (IsSystem ? "-System" : string.Empty));
        lblDescription = Translator.Instant("Pages.SearchEngines.Labels.PageDescription" + (IsSystem ? "-System" : string.Empty));
        var service = new SearchEngineService();
        Items = (IsSystem ? service.GetAllSystem() : service.GetAllForUser(UserUid))
            .OrderBy(x => x.Name).ToList();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Physically deletes a search engine
    /// </summary>
    /// <param name="item">the search engine being deleted</param>
    /// <returns>true if the engine was deleted or not</returns>
    protected override bool DoDelete(Models.SearchEngine item)
    {
        Items.RemoveAll(x => x.Uid == item.Uid);
        new SearchEngineService().Delete(item.Uid);
        Table.SetData(Items);
        return true;
    }

    /// <summary>
    /// Edits a search engine
    /// </summary>
    /// <param name="engine">the search engine being edited</param>
    private async Task Edit(SearchEngine engine)
    {
        var result = await Popup.OpenEditor<SearchEngineEditor, SearchEngine>(Translator, engine, new ()
        {
            { nameof(SearchEngineEditor.IsSystem), IsSystem },
            { nameof(SearchEngineEditor.Settings), Settings }
        });
        if (result.Success == false)
            return;
        engine.Name = result.Data.Name;
        engine.Url = result.Data.Url;
        engine.Shortcut = result.Data.Shortcut;
        engine.Icon = result.Data.Icon;
        Items = Items.OrderBy(x => x.Name).ToList();
        Table.SetData(Items);
    }
    
    /// <summary>
    /// Adds a new search engine
    /// </summary>
    protected override async Task Add()
    {
        var result = await Popup.OpenEditor<SearchEngineEditor, SearchEngine>(Translator, null, new ()
        {
            { nameof(SearchEngineEditor.IsSystem), IsSystem },
            { nameof(SearchEngineEditor.Settings), Settings }
        });
        if (result.Success == false)
            return;
        Items.Add(result.Data);
        Items = Items.OrderBy(x => x.Name).ToList();
        Table.SetData(Items);
        
    }

    /// <summary>
    /// Sets "Default" for a search engine
    /// </summary>
    /// <param name="engine">the engine to update</param>
    /// <param name="isDefault">the default state</param>
    private void SetDefault(SearchEngine engine, bool isDefault)
    {
        var service = new SearchEngineService();
        if (isDefault) 
        {
            // only clear other defaults when setting this one as a default
            foreach (var item in this.Items)
            {
                if (item.IsDefault && item.Uid != engine.Uid)
                {
                    // remove the default
                    item.IsDefault = false;
                    service.Update(item);
                }
            }
        }

        if (engine.IsDefault != isDefault)
        {
            engine.IsDefault = isDefault;
            service.Update(engine);
        }
        Table.TriggerStateHasChanged();
    }

    /// <summary>
    /// Sets if a search engine is enabled
    /// </summary>
    /// <param name="engine">the engine to update</param>
    /// <param name="enabled">if the engine is enabled or not</param>
    private void SetEnabled(SearchEngine engine, bool enabled)
    {
        if (engine.Enabled == enabled)
            return; // nothing to do
        engine.Enabled = enabled;
        new SearchEngineService().Update(engine);
        Table.TriggerStateHasChanged();
    }
}