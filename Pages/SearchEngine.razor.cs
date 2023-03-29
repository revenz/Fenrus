using Fenrus.Models;
using Fenrus.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509;

namespace Fenrus.Pages;

/// <summary>
/// Search engine page
/// </summary>
public partial class SearchEngine: UserPage
{
    Models.SearchEngine Model { get; set; } = new();
    
    private string Icon { get; set; }

    private string InitialImage;

    private string lblTitle, lblNameHelp, lblUrl, lblUrlHelp, lblShortcut, lblShortcutHelp, lblIcon;

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


    protected override Task PostGotUser()
    {
        lblTitle = Translator.Instant("Pages.SearchEngine.Title" + (IsSystem ? "-System": string.Empty));
        lblNameHelp = Translator.Instant("Pages.SearchEngine.Fields.Name-Help");
        lblUrl = Translator.Instant("Pages.SearchEngine.Fields.Url");
        lblUrlHelp = Translator.Instant("Pages.SearchEngine.Fields.Url-Help");
        lblShortcut = Translator.Instant("Pages.SearchEngine.Fields.Shortcut");
        lblShortcutHelp = Translator.Instant("Pages.SearchEngine.Fields.Shortcut-Help");
        lblIcon = Translator.Instant("Pages.SearchEngine.Fields.Icon");
        
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
            Model = new SearchEngineService().GetByUid(Uid);
            if (Model == null || Model.UserUid != (IsSystem ? Guid.Empty : UserUid))
            {
                Router.NavigateTo(IsSystem ? "/settings/system/search-engines" : "/settings/search-engines");
                return Task.CompletedTask;
            }
            
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

        return Task.CompletedTask;
    }

    void Save()
    {
        string saveImage = ImageHelper.SaveImageFromBase64(Icon);

        var service = new SearchEngineService();
        if (isNew)
        {
            Model.Icon = saveImage;
            Model.Uid = Guid.NewGuid();
            Model.IsSystem = IsSystem;
            Model.UserUid = IsSystem ? Guid.Empty : UserUid;
            service.Add(Model);
        }
        else
        {
            var existing = service.GetByUid(Model.Uid);
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
            service.Update(Model);
        }
        if(IsSystem == false)
            Router.NavigateTo("/settings/search-engines");  
    }

    void Cancel()
    {
        this.Router.NavigateTo("/settings/search-engines");
    }
}