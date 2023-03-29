namespace Fenrus.Components;

using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

public partial class FenrusTabs : ComponentBase
{
    [Parameter]
    public RenderFragment ChildContent { get; set; }
    public FenrusTab ActiveTab { get; internal set; }

    List<FenrusTab> Tabs = new List<FenrusTab>();

    internal void AddTab(FenrusTab tab)
    {
        if (Tabs.Contains(tab) == false)
        {
            Tabs.Add(tab);
            this.StateHasChanged();
        }

        if (ActiveTab == null)
            ActiveTab = tab;
    }

    private void SelectTab(FenrusTab tab)
    {
        this.ActiveTab = tab;
    }

    /// <summary>
    /// Selects the first tab
    /// </summary>
    public void SelectFirstTab()
    {
        var tab = Tabs.FirstOrDefault(x => x.Visible);
        if (tab == null)
            return;
        this.ActiveTab = tab;
        this.StateHasChanged();
    }

    /// <summary>
    /// Triggers state has changed
    /// </summary>
    public void TriggerStateChanged()
        => StateHasChanged();
}
