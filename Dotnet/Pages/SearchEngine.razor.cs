using Fenrus.Services;
using Microsoft.AspNetCore.Components;

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

    private bool isNew = false;
    

    protected override async Task PostGotUser()
    {
        if (Uid == Guid.Empty)
        {
            // new search engine
            isNew = true;
            Model = new();
        }
        else
        {
            isNew = false;
            Model = Settings.SearchEngines.First(x => x.Uid == Uid);
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
            Settings.SearchEngines.Add(Model);
        }
        else
        {
            var existing = Settings.SearchEngines.First(x => x.Uid == Uid);
            existing.Enabled = Model.Enabled;
            existing.IsDefault = Model.IsDefault;
            existing.IsSystem = Model.IsSystem;
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
        }
        Settings.Save();
        this.Router.NavigateTo("/settings/search-engines");
    }

    void Cancel()
    {
        this.Router.NavigateTo("/settings/search-engines");
    }
}