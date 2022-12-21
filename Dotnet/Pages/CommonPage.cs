using Fenrus.Components.Dialogs;
using Fenrus.Models;
using Humanizer;
using Microsoft.AspNetCore.Components;

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
    /// Removes an item, prompting for confirmation
    /// </summary>
    /// <param name="item">The item being removed</param>
    protected async Task Remove(T item)
    {
        if (await Confirm.Show("Delete", $"Delete {GetTypeName()} '{item.Name}'?") == false)
            return;
        Console.WriteLine("deleting: " + item.Name);
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
        Router.NavigateTo($"/settings/{GetTypeNameRoute()}/" + Guid.Empty);   
    }

    protected async Task Move(T item, bool up)
    {
        
    }
}