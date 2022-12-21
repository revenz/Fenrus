using Fenrus.Components.Dialogs;
using Fenrus.Models;
using Humanizer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Pages;

/// <summary>
/// Common page
/// </summary>
/// <typeparam name="T">the page this item lists</typeparam>
public abstract class CommonPage<T>: UserPage
where T : IModal
{
    protected string GetTypeName()
        => typeof(T).Name.Humanize(LetterCasing.LowerCase);
    protected string GetTypeNameRoute()
        => typeof(T).Name.Pluralize().Underscore().Hyphenate();
    
    /// <summary>
    /// Gets or sets if this is a system search engine
    /// </summary>
    [FromQuery]
    protected bool IsSystem { get; set; }
    
    /// <summary>
    /// Removes an item, prompting for confirmation
    /// </summary>
    /// <param name="item">The item being removed</param>
    protected async Task Remove(T item)
    {
        if (await Confirm.Show("Delete", $"Delete {GetTypeName()} '{item.Name}'?") == false)
            return;
        DoDelete(item);
    }

    /// <summary>
    /// Actually performs the deletion after confirmation has been received
    /// </summary>
    /// <param name="item">the item being deleted</param>
    protected virtual void DoDelete(T item)
        => DbHelper.Delete<T>(item.Uid);

    protected virtual async Task Add()
    {
        Router.NavigateTo($"/settings/{GetTypeNameRoute()}/" + Guid.Empty + 
                          (IsSystem ? "isSystem=true" : string.Empty));   
    }

    protected async Task Move(T item, bool up)
    {
        
    }
}