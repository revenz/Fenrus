using Fenrus.Models;
using Fenrus.Models.UiModels;
using Fenrus.Services;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Panel for user settings
/// </summary>
public partial class PanelUserSettings : ComponentBase
{
    /// <summary>
    /// Gets or sets the dashboards in the system
    /// </summary>
    [Parameter] public List<Dashboard> Dashboards { get; set; }

    /// <summary>
    /// Gets or sets the user settings
    /// </summary>
    [Parameter] public UserSettings Settings { get; set; }
    
    /// <summary>
    /// Gets or sets the system search engines
    /// </summary>
    [Parameter] public List<Models.SearchEngine> SystemSearchEngines { get; set; }
    
    /// <summary>
    /// Gets or sets the current dashboard
    /// </summary>
    [Parameter] public Dashboard Dashboard { get; set; }

    /// <summary>
    /// Gets or sets a list of all themes in the system
    /// </summary>
    [Parameter] public List<string> Themes { get; set; }
    
    /// <summary>
    /// Gets or set the current theme
    /// </summary>
    [Parameter] public Theme Theme { get; set; }

    /// <summary>
    /// Gets or sets the page helper
    /// </summary>
    [Parameter] public PageHelper PageHelper { get; set; }

    /// <summary>
    /// Gets the translator
    /// </summary>
    private Translator Translator => PageHelper.Translator;

    private List<ListOption> Languages;
    protected override void OnInitialized()
    {
        Languages = Translator.GetLanguages().Select(x => new ListOption(){ Value = x.Value, Label = x.Key}).ToList();
    }
}