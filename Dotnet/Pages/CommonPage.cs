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
    
    protected async Task Remove(T item)
    {
        if (await Confirm.Show("Delete", $"Delete {GetTypeName()} '{item.Name}'?") == false)
            return;
        Console.WriteLine("deleting: " + item.Name);
    }

    protected async Task Add()
    {
        
    }

    protected async Task Move(T item, bool up)
    {
        
    }
}