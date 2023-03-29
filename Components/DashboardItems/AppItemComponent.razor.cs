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
    /// Gets or sets if this is a guest dashboard
    /// </summary>
    [Parameter] public bool IsGuest { get; set; }

    /// <summary>
    /// Gets or sets the settings
    /// </summary>
    [Parameter] public UserSettings Settings { get; set; }

    /// <summary>
    /// Gets or sets the page helper
    /// </summary>
    [CascadingParameter] public PageHelper PageHelper { get; set; }

    /// <summary>
    /// Gets or sets the current dashboard
    /// </summary>
    [CascadingParameter] public Dashboard Dashboard { get; set; }

    /// <summary>
    /// Gets the up-time states of URLs
    /// </summary>
    [Parameter] public Dictionary<string, int> UpTimeStates { get; init; }
    
    private FenrusApp App { get; set; }

    private string Css;
    private string Target, OnClickCode, SerializedJsSafeModel, Icon, Title;
    private string isUpClass = string.Empty;

    protected override void OnInitialized()
    {
        Target = Model.Target?.EmptyAsNull() ?? Dashboard?.LinkTarget?.EmptyAsNull() ?? "_self";
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

        if (Model.Monitor && string.IsNullOrWhiteSpace(Model.Url) == false && UpTimeStates?.TryGetValue(Model.ApiUrl?.EmptyAsNull() ?? Model.Url, out int state) == true)
            Css += "is-up-" + (state == 0 ? "false" : state == 1 ? "true" : state) + " ";

        Icon = GetIcon();

        if (App.IsSmart)
        {
            int interval = App.Interval;
            if (interval > 0)
                interval *= 1000; // convert seconds to milliseconds

            PageHelper.RegisterScriptBlock($@"document.addEventListener('DOMContentLoaded', function(event) {{ 
    LiveApp('{App.Name}', '{Model.Uid}', {interval});
}});");
        }
    }

    /// <summary>
    /// Gets the Icon URL
    /// </summary>
    /// <returns>the Icon URL</returns>
    private string GetIcon()
    { 
        if (string.IsNullOrWhiteSpace(Model.Icon) != true)
        {
            if (Model.Icon.StartsWith("db:/image/"))
                return "/fimage/" + Model.Icon["db:/image/".Length..] + "?version=" + Globals.Version;
            return Model.Icon + "?version=" + Globals.Version;
        }

        if (string.IsNullOrEmpty(App.Icon) == false)
            return $"/apps/{Uri.EscapeDataString(App.Name)}/{App.Icon}?version=" + Globals.Version;
        return $"/favicon?color={@Uri.EscapeDataString(Dashboard.AccentColor)}&version={Globals.Version}";
    }
}