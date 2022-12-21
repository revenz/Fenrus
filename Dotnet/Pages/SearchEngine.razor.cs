using Fenrus.Models;
using Fenrus.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Pages;

/// <summary>
/// Search engine page
/// </summary>
public partial class SearchEngine: UserPage
{
    Models.SearchEngine Model { get; set; } = new();
    
    private string Icon { get; set; }

    private string InitialImage;

    [Parameter]
    public string UidString
    {
        get => Uid.ToString();
        set
        {
            if (Guid.TryParse(value, out Guid guid))
                this.Uid = guid;
            else
            {
                // do a redirect
            }
        }
    }

    /// <summary>
    /// Gets or sets the UID of the group
    /// </summary>
    public Guid Uid { get; set; }
    
    /// <summary>
    /// Gets or sets if this is a system search engine
    /// </summary>
    [FromQuery]
    protected bool IsSystem { get; set; }

    private bool isNew = false;

    private Models.SearchEngine GetSearchEngine()
    {
        if (IsSystem)
            return DbHelper.GetByUid<Models.SearchEngine>(Uid);
        return Settings.SearchEngines.First(x => x.Uid == Uid);
    }

    protected override async Task PostGotUser()
    {
        if (Uid == Guid.Empty)
        {
            // new search engine
            isNew = true;
            Model = new();
            if (IsSystem)
                Model.IsSystem = true;
        }
        else
        {
            isNew = false;
            Model = GetSearchEngine();
            
            if (string.IsNullOrEmpty(Model.Icon) == false)
            {
                if (Model.Icon.StartsWith("db:/image/"))
                {
                    // db image
                    InitialImage = "/fimage/" + Model.Icon["db:/image/".Length..];
                }
                else
                {
                    InitialImage = Model.Icon;
                }
            }
        }
    }

    void Save()
    {
        string saveImage = ImageHelper.SaveImageFromBase64(Icon);
        
        if (isNew)
        {
            Model.Icon = saveImage;
            Model.Uid = Guid.NewGuid();
            Model.IsSystem = IsSystem;
            if (IsSystem)
            {
                DbHelper.Insert(Model);
                return;
            }
            Settings.SearchEngines.Add(Model);
        }
        else
        {
            var existing = GetSearchEngine();
            existing.Enabled = Model.Enabled;
            existing.IsDefault = Model.IsDefault;
            existing.IsSystem = IsSystem;
            if (string.IsNullOrEmpty(saveImage) == false)
            {
                // delete existing icon
                ImageHelper.DeleteImage(existing.Icon);
                // now set the new one
                existing.Icon = saveImage;
            }

            existing.Name = Model.Name;
            existing.Shortcut = Model.Shortcut;
            existing.Url = Model.Url;
            if (IsSystem)
            {
                DbHelper.Update(existing);
                return;
            }
        }
        Settings.Save();
        Router.NavigateTo("/settings/search-engines");
    }

    void Cancel()
    {
        this.Router.NavigateTo("/settings/search-engines");
    }
}