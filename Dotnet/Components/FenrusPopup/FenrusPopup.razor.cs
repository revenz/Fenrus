using Fenrus.Components.SideEditors;
using Fenrus.Models;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Fenrus popup, this is the component responsible for rendering side editors and modals
/// </summary>
public partial class FenrusPopup
{
    private List<FenrusPopupItem> Popups = new();

    public void Show<T>()
    {
        
    }

    public Task<PopupResult<GroupItem>> GroupItemEditor(GroupItem item)
    {
        TaskCompletionSource<PopupResult<GroupItem>> task = new ();
        FenrusPopupItem popup = null;
        popup = new()
        {
            Type = typeof(GroupItemEditor),
            Parameters = new()
            {
                { nameof(SideEditors.GroupItemEditor.Item), item },
                {
                    nameof(SideEditors.GroupItemEditor.OnSaved),
                    EventCallback.Factory.Create<GroupItem>(this, model =>
                    {
                        Popups.Remove(popup);
                        StateHasChanged();
                        task.SetResult(new() { Data = model, Success = true });
                    })
                },
                {
                    nameof(SideEditors.GroupItemEditor.OnCanceled),
                    EventCallback.Factory.Create(this, _ =>
                    {
                        Popups.Remove(popup);
                        StateHasChanged();
                        task.SetResult(new() { Success = false });
                    })
                }
            }
        };
        Popups.Add(popup);
        StateHasChanged();
        return task.Task;
    }
}

class FenrusPopupItem
{
    public int Level { get; set; }
    public Type Type { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
}

/// <summary>
/// A popup result
/// </summary>
/// <typeparam name="T">the type of data that is returned</typeparam>
public class PopupResult<T>
{
    /// <summary>
    /// The success status of the popup
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// The data with the popup
    /// </summary>
    public T Data { get; set; }
}