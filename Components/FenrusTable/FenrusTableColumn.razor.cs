using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Fenrus table column
/// </summary>
public partial class FenrusTableColumn<TItem>:ComponentBase
{
    /// <summary>
    /// Gets or sets the table this column belongs
    /// </summary>
    [CascadingParameter] FenrusTable<TItem> Table { get; set; }

    /// <summary>
    /// Gets or sets the header content
    /// </summary>
    [Parameter] public RenderFragment Header { get; set; }

    /// <summary>
    /// Gets or sets the cell content
    /// </summary>
    [Parameter] public RenderFragment<TItem> Cell { get; set; }
    
    /// <summary>
    /// Gets or sets if this column is hidden
    /// </summary>
    [Parameter] public bool Hidden { get; set; }
    
    /// <summary>
    /// Gets or sets if this content should be formatted as a pre
    /// </summary>
    [Parameter] public bool Pre { get; set; }

    string _Width = string.Empty;
    string className = "fillspace";
    string style = string.Empty;
    
    /// <summary>
    /// Gets or sets the width of this column
    /// </summary>
    [Parameter]
    public string Width
    {
        get => _Width;
        set
        {
            _Width = value ?? string.Empty;
            if (_Width != string.Empty) {
                className = string.Empty;
            }
        }
    }

    /// <summary>
    /// Gets or sets the width when viewed on a mobile device
    /// </summary>
    [Parameter]
    public string MobileWidth { get; set; }

    private string _MinWidth = string.Empty;    
    
    /// <summary>
    /// Gets or sets the minimum width
    /// </summary>
    [Parameter]
    public string MinWidth
    {
        get => _MinWidth;
        set
        {
            _MinWidth = value ?? string.Empty;
            if (_MinWidth == string.Empty)
                style = string.Empty;
            else
                style = $"min-width: {_MinWidth};";

        }
    }

    /// <summary>
    /// Gets or sets the column anme
    /// </summary>
    [Parameter]
    public string ColumnName { get; set; }


    /// <summary>
    /// Gets the classname
    /// </summary>
    public string ClassName => className;
    /// <summary>
    /// Gets the style
    /// </summary>
    public string Style => style;

    protected override void OnInitialized()
    {
        this.Table.AddColumn(this);
    }

    
}