using Fenrus.Components.SideEditors;
using Fenrus.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;

namespace Fenrus.Components;

/// <summary>
/// Fenrus popup, this is the component responsible for rendering side editors and modals
/// </summary>
public partial class FenrusPopup
{
    private List<FenrusPopupItem> Popups = new();

    /// <summary>
    /// Opens a group item editor for new items
    /// This differs from the other editor as this will yield multiple items back if kept open
    /// </summary>
    /// <param name="translator">The translator to use</param>
    /// <param name="isSystem">if this is for a system group or not</param>
    /// <param name="user">the user</param>
    /// <returns>a async list of items being added, each item is returned as it is added</returns>
    public async IAsyncEnumerable<PopupResult<GroupItem>> GroupItemEditorNew(Translator translator, bool isSystem, User user)
    {
        bool done = false;
        TaskCompletionSource<PopupResult<GroupItem>> task = new ();
        #region popup
        FenrusPopupItem popup = new()
        {
            Type = typeof(GroupItemEditor),
            Parameters = new()
            {
                { nameof(SideEditors.GroupItemEditor.Translator), translator },
                { nameof(SideEditors.GroupItemEditor.IsSystem), isSystem },
                { nameof(SideEditors.GroupItemEditor.User), user },
                { nameof(SideEditors.GroupItemEditor.Item), null },
                {
                    // this event fires, but keeps the editor opened, so we're not done yet
                    nameof(SideEditors.GroupItemEditor.OnSavedKeepOpen),
                    EventCallback.Factory.Create<GroupItem>(this, model =>
                    {
                        task.SetResult(new() { Data = model, Success = true });
                    })
                },
                {
                    nameof(SideEditors.GroupItemEditor.OnSaved),
                    EventCallback.Factory.Create<GroupItem>(this, model =>
                    {
                        done = true;
                        task.SetResult(new() { Data = model, Success = true });
                    })
                },
                {
                    nameof(SideEditors.GroupItemEditor.OnCanceled),
                    EventCallback.Factory.Create(this, _ =>
                    {
                        done = true;
                        task.SetResult(new() { Success = false });
                    })
                }
            }
        };
        #endregion
        Popups.Add(popup);
        StateHasChanged();
        while (done == false)
        {
            var result = await task.Task.WaitAsync(CancellationToken.None);
            if (done)
            {
                // editor is completed, we can remove this popup and close
                Popups.Remove(popup);
                StateHasChanged();
            }
            else
            {
                // editor is kept open, so we need a new task to await 
                task = new();
            }

            yield return result;
        }
    }
    /// <summary>
    /// Opens a group item editor
    /// </summary> 
    /// <param name="translator">The translator to use</param>
    /// <param name="item">the group item to edit</param>
    /// <param name="isSystem">if this is for a system group or not</param>
    /// <param name="user">the user</param>
    /// <returns>the result of the editor</returns>
    public Task<PopupResult<GroupItem>> GroupItemEditor(Translator translator, GroupItem item, bool isSystem, User user)
    {
        TaskCompletionSource<PopupResult<GroupItem>> task = new ();
        FenrusPopupItem popup = null;
        popup = new()
        {
            Type = typeof(GroupItemEditor),
            Parameters = new()
            {
                { nameof(SideEditors.GroupItemEditor.Translator), translator },
                { nameof(SideEditors.GroupItemEditor.IsSystem), isSystem },
                { nameof(SideEditors.GroupItemEditor.User), user },
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
    
    
    /// <summary>
    /// Opens a generic editor
    /// </summary>
    /// <param name="item">the item to edit</param>
    /// <param name="translator">The translator to use</param>
    /// <typeparam name="T">the type of editor to open</typeparam>
    /// <typeparam name="U">the type of model being edited</typeparam>
    /// <param name="additionalParameters">[Optional] additional parameters to pass to the popup</param>
    /// <returns>the open result</returns>
    public Task<PopupResult<U>> OpenEditor<T, U>(Translator translator, U item, Dictionary<string, object> additionalParameters = null)
    {
        TaskCompletionSource<PopupResult<U>> task = new ();
        FenrusPopupItem popup = null;
        
        var parameters = additionalParameters ?? new ();
        parameters.Add("Item", item);
        parameters.Add(nameof(SideEditorBase.Translator), translator);
        parameters.Add("OnSaved",
            EventCallback.Factory.Create<U>(this, model =>
            {
                Popups.Remove(popup);
                StateHasChanged();
                task.SetResult(new() { Data = model, Success = true });
            }));
        parameters.Add("OnCanceled",
            EventCallback.Factory.Create(this, _ =>
            {
                Popups.Remove(popup);
                StateHasChanged();
                task.SetResult(new() { Success = false });
            }));
        popup = new()
        {
            Type = typeof(T),
            Parameters = parameters
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