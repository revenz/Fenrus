namespace Fenrus.Components;

using Microsoft.AspNetCore.Components;

public partial class FenrusTab:ComponentBase
{
    [CascadingParameter] FenrusTabs Tabs { get; set; }

    [Parameter] public string Title { get; set; }

    private bool _Visible = true;
    [Parameter]
    public bool Visible
    {
        get => _Visible;
        set
        {
            if (value == _Visible) return;
            _Visible = value;
            Tabs?.TriggerStateChanged();
        }
    } 

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    protected override void OnInitialized()
    {
        Tabs.AddTab(this);
    }

    private bool IsActive() => this.Tabs.ActiveTab == this;
}
