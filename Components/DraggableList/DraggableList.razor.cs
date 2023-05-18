using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Fenrus.Components;

/// <summary>
/// Represents a draggable list component.
/// </summary>
/// <typeparam name="TItem">The type of items in the list.</typeparam>
public partial class DraggableList<TItem> : ComponentBase
{
    /// <summary>
    /// Gets or sets the list of items in the draggable list.
    /// </summary>
    [Parameter]
    public List<TItem> Items { get; set; }

    /// <summary>
    /// Gets or sets the CSS class name for the draggable list container.
    /// </summary>
    [Parameter]
    public string CssClassName { get; set; }

    /// <summary>
    /// Gets or sets the item template for rendering each item in the draggable list.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem> ItemTemplate { get; set; }

    private TItem draggingItem;

    /// <summary>
    /// Called after the component has been initialized but before it has been rendered
    /// </summary>
    protected override void OnInitialized()
    {
        if (Items == null)
        {
            Items = new List<TItem>();
        }
    }

    /// <summary>
    /// Handles the drag start event for an item in the draggable list.
    /// </summary>
    /// <param name="item">The item being dragged.</param>
    protected void DragStart(TItem item)
    {
        draggingItem = item;
    }

    /// <summary>
    /// Handles the drag enter event for an item in the draggable list.
    /// </summary>
    /// <param name="item">The item being dragged over.</param>
    protected void DragEnter(TItem item)
    {
        if (!EqualityComparer<TItem>.Default.Equals(item, draggingItem))
        {
            var draggingIndex = Items.IndexOf(draggingItem);
            var targetIndex = Items.IndexOf(item);
            if (targetIndex < 0 || draggingIndex < 0)
                return; // can happen if attempting to drag across two draggable lists

            Items.RemoveAt(draggingIndex);
            Items.Insert(targetIndex, draggingItem);
        }
    }

    /// <summary>
    /// Handles the drop event for an item in the draggable list.
    /// </summary>
    /// <param name="e">The drag event arguments.</param>
    /// <param name="item">The item where the drop occurred.</param>
    protected void Drop(DragEventArgs e, TItem item)
    {
        draggingItem = default;
    }

    /// <summary>
    /// Handles the blur event for an item in the draggable list.
    /// </summary>
    /// <param name="item">The item that lost focus.</param>
    protected void Blur(TItem item)
    {
        if (EqualityComparer<TItem>.Default.Equals(item, draggingItem))
        {
            draggingItem = default;
        }
    }

    /// <summary>
    /// Gets the tab index for an item in the draggable list.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The tab index value.</returns>
    protected int GetTabIndex(TItem item)
    {
        return EqualityComparer<TItem>.Default.Equals(item, draggingItem) ? 0 : -1;
    }
}