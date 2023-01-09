using System.Text.Encodings.Web;
using System.Web;
using Fenrus.Models;
using Fenrus.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Fenrus.Components.DashboardItems;

/// <summary>
/// App item component
/// </summary>
public partial class AppItemComponent
{
    /// <summary>
    /// Gets or sets the link item model
    /// </summary>
    [Parameter] public AppItem Model { get; set; }

    /// <summary>
    /// Gets or sets the settings
    /// </summary>
    [Parameter] public UserSettings Settings { get; set; }

    /// <summary>
    /// Gets or sets the page helper
    /// </summary>
    [CascadingParameter] public PageHelper PageHelper { get; set; }

    private FenrusApp App { get; set; }

    private string Css;
    private string Target, OnClickCode, SerializedJsSafeModel, Icon, Title;

    protected override void OnInitialized()
    {
        Target = Model.Target?.EmptyAsNull() ?? Settings.LinkTarget?.EmptyAsNull() ?? "_self";
        SerializedJsSafeModel = JsonSerializer.Serialize(Model).Replace("'", "\\'");
        OnClickCode = Target == "IFrame"
            ? $"openIframe(event, '{SerializedJsSafeModel}'); return false;"
            : $"launch(event, '{Model.Uid}')";
        
        App = AppService.GetByName(Model.AppName);
        Title = Model.Name?.EmptyAsNull() ?? App.Name;
        Css = Model.Size.ToString().ToLower().Replace("xl", "x-l") + " ";
        if(App.IsSmart)
            Css += "db-smart ";
        if(App.Carousel && Model.Size >= ItemSize.Large)
            Css += "carousel ";
        if (Model.Target == "IFrame")
            Css += "iframe ";
        // if(string.IsNullOrEmpty(Model.SshServer) == false)
        //     Css += "ssh ";
        // if(string.IsNullOrEmpty(Model.DockerUid) == false)
        //     Css += "docker ";


        Icon = (Model.Icon?.EmptyAsNull() ??
                (string.IsNullOrEmpty(App.Icon) == false
                    ? $"/apps/{Uri.EscapeDataString(App.Name)}/{App.Icon}"
                    : "/favicon.svg")
            ) + "?version=" + Globals.Version;

        int interval = App.Interval;
        if (interval > 0)
            interval *= 1000; // convert seconds to milliseconds

        PageHelper.RegisterScriptBlock($@"document.addEventListener('DOMContentLoaded', function(event) {{ 
    LiveApp('{App.Name}', '{Model.Uid}', {interval});
}});");
    }
}