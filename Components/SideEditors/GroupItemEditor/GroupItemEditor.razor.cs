using Blazored.Toast.Services;
using Fenrus.Controllers;
using Fenrus.Helpers.AppHelpers;
using Fenrus.Models;
using Fenrus.Models.UiModels;
using Jint;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.SideEditors;

/// <summary>
/// Group Item Editor
/// </summary>
public partial class GroupItemEditor : SideEditorBase, IDisposable
{
    /// <summary>
    /// Gets if this item is has loaded, and if not, some setting should not be overriden, eg Size
    /// </summary>
    private bool Loaded { get; set; }
    /// <summary>
    /// Gets or sets the toast service
    /// </summary>
    [Inject] protected IToastService ToastService { get; set; }
    /// <summary>
    /// Gets or sets the item this is editing, leave null for a new item
    /// </summary>
    [Parameter] public GroupItem Item { get; set; }
    /// <summary>
    /// Gets or sets the callback when this editor is saved (and not kept open)
    /// </summary>
    [Parameter] public EventCallback<GroupItem> OnSaved { get; set; }
    /// <summary>
    /// Event that is called when saving but keeping open
    /// </summary>
    [Parameter] public EventCallback<GroupItem> OnSavedKeepOpen { get; set; }
    /// <summary>
    /// Gets or sets the callback when this editor is canceled
    /// </summary>
    [Parameter] public EventCallback OnCanceled { get; set; }

    /// <summary>
    /// Gets or sets the bound model the user is editing
    /// </summary>
    private GroupItemEditorModel Model;
    /// <summary>
    /// Gets or sets the title of the editor
    /// </summary>
    private string Title;
    /// <summary>
    /// Gets or sets if this is an editor for a new item
    /// </summary>
    private bool IsNew;
    /// <summary>
    /// Gets or sets if this editor should be kept open after saving
    /// Note: only available to new items
    /// </summary>
    private bool KeepOpen { get; set; }

    /// <summary>
    /// Gets or sets all the apps, indexed by their names
    /// </summary>
    private Dictionary<string, FenrusApp> Apps;
    /// <summary>
    /// Gets or sets a list of smart apps and basic apps
    /// </summary>
    private List<ListOption> SmartApps, BasicApps;

    /// <summary>
    /// Gets or sets the app selector input control
    /// </summary>
    private Inputs.InputSelect<string> AppSelector { get; set; }

    /// <summary>
    /// Gets or sets the side editor instance
    /// </summary>
    private SideEditor Editor { get; set; }

    /// <summary>
    /// Gets or sets the selected app
    /// Note: can be null if not editing an app or none is selected
    /// </summary>
    private FenrusApp? SelectedApp;

    /// <summary>
    /// Gets or sets if the test of this app should be debugged and shown to the user
    /// </summary>
    private bool Debug = false;

    /// <summary>
    /// Gets or sets the selected app name
    /// </summary>
    private string SelectedAppName
    {
        get => SelectedApp?.Name;
        set
        {
            if (Model.Name == SelectedApp?.Name)
                Model.Name = string.Empty; // clear it since we are changing apps
            if (Model.Url == SelectedApp?.DefaultUrl)
                Model.Url = "https://"; // clear it since we are changing apps
            if (value != null && Apps.ContainsKey(value))
            {
                SelectedApp = Apps[value];
                if (string.IsNullOrEmpty(Model.Name))
                    Model.Name = SelectedApp.Name; // make the name the app name if empty
                if (string.IsNullOrWhiteSpace(SelectedApp.DefaultUrl) == false &&
                    string.IsNullOrWhiteSpace(Model.Url) || Model.Url == "https://" || Model.Url == "http://")
                    Model.Url = SelectedApp.DefaultUrl; // update the apps default url
                if (Loaded)
                {
                    // set the default size
                    Model.Size = SelectedApp.DefaultSize ?? ItemSize.Medium;
                }

                InitSelectedApp();
            }
            else
                SelectedApp = null;
            Model.AppName = value;
        }
    }

    private FenrusTabs Tabs;

    private void InitSelectedApp()
    {
        if (SelectedApp == null)
            return;
        if (SelectedApp.Properties?.Any() == true)
        {
            foreach (var prop in SelectedApp.Properties)
            {
                if (prop.Options?.Any() != true)
                    continue;
                foreach (var opt in prop.Options)
                {
                    if(opt.Value is JsonElement je)
                    {
                        if (je.ValueKind == JsonValueKind.False)
                            opt.Value = false;
                        else if (je.ValueKind == JsonValueKind.True)
                            opt.Value = true;
                        else if (je.ValueKind == JsonValueKind.Number)
                            opt.Value = je.GetInt32();
                        else
                            opt.Value = je.ToString();
                    }
                }
            }
        }
    }

    private string lblKeepOpen, lblAdd, lblCancel, lblSave, lblSshDescription, 
        lblDockerDescription, lblDockerTerminalDescription, 
        lblGeneral, lblProperties, lblDocker, lblInfo, lblSsh,
        lblAuthor, lblApplicationUrl, lblTest, lblTesting, lblDescription,
        lblTestSuccesful, lblTestFailed;

    private List<ListOption> DockerServers;
    protected override void OnInitialized()
    {
        lblAdd = Translator.Instant("Labels.Add");
        lblSave = Translator.Instant("Labels.Save");
        lblCancel = Translator.Instant("Labels.Cancel");
        lblGeneral = Translator.Instant("Pages.GroupItem.Tabs.General");
        lblProperties = Translator.Instant("Pages.GroupItem.Tabs.Properties");
        lblSsh = Translator.Instant("Pages.GroupItem.Tabs.SSH");
        lblDocker = Translator.Instant("Pages.GroupItem.Tabs.Docker");
        lblInfo = Translator.Instant("Pages.GroupItem.Tabs.Info");
        lblKeepOpen = Translator.Instant("Pages.GroupItem.Labels.KeepOpen");  
        lblSshDescription = Translator.Instant("Pages.GroupItem.Labels.SshDescription");  
        lblDockerDescription = Translator.Instant("Pages.GroupItem.Labels.DockerDescription"); 
        lblDockerTerminalDescription = Translator.Instant("Pages.GroupItem.Labels.DockerTerminalDescription");  
        lblAuthor = Translator.Instant("Pages.GroupItem.Labels.Author");
        lblDescription = Translator.Instant("Pages.GroupItem.Labels.Description");
        lblApplicationUrl = Translator.Instant("Pages.GroupItem.Labels.ApplicationUrl");  
        lblTest = Translator.Instant("Labels.Test");  
        lblTesting = Translator.Instant("Labels.Testing");
        lblTestSuccesful = Translator.Instant("Pages.GroupItem.Labels.TestSuccessful");
        lblTestFailed = Translator.Instant("Pages.GroupItem.Labels.TestFailed");
            
        Apps = AppService.Apps;
        SmartApps = Apps.Where(x => x.Value.IsSmart == true).OrderBy(x => x.Key)
            .Select(x => new ListOption() { Label = x.Key, Value = x.Key }).ToList();
        
        BasicApps = Apps.Where(x => x.Value.IsSmart == false).OrderBy(x => x.Key)
            .Select(x => new ListOption() { Label = x.Key, Value = x.Key }).ToList();

        var settings = new SystemSettingsService().Get();

        DockerServers = new DockerService().GetAll()?.OrderBy(x => x.Name)?.Select(x => new ListOption()
        {
            Label = x.Name,
            Value = x.Uid
        })?.ToList() ?? new ();
        if (DockerServers.Any())
        {
            DockerServers.Insert(0, new ListOption()
            {
                Label = Translator.Instant("Labels.None"),
                Value = Guid.Empty
            });
        }
        
        this.Model = new();
        Title = "Edit Item";
        IsNew = false;
        KeepOpen = false;
        if (Item is AppItem app)
        {
            Model.ItemType = app.Type;
            Model.Target = app.Target;
            Model.Url = app.Url;
            Model.ApiUrl = app.ApiUrl;
            Model.AppName = app.AppName;
            Model.DockerContainer = app.DockerContainer;
            Model.DockerUid = app.DockerUid;
            Model.DockerCommand = app.DockerCommand;
            Model.SshPasswordOriginal = app.SshPassword;
            Model.SshPassword = string.IsNullOrEmpty(app.SshPassword) ? string.Empty : Globals.DUMMY_PASSWORD;
            Model.SshServer = app.SshServer;
            Model.SshUserName = app.SshUserName;
            Model.Icon = app.Icon;
            Model.Name = app.Name;
            Model.Size = app.Size;
            Model.Uid = app.Uid;
            Model.Monitor = app.Monitor;
            Model.Properties = app.Properties ?? new ();
            // SelectedApp = Apps.ContainsKey(app.AppName) ? Apps[app.AppName] : null;
        }
        else if (Item is LinkItem link)
        {
            Model.ItemType = link.Type;
            Model.Target = link.Target;
            Model.Url = link.Url;
            Model.Icon = link.Icon;
            Model.Name = link.Name;
            Model.Size = link.Size;
            Model.Uid = link.Uid;
            Model.Monitor = link.Monitor;
        }
        else
        {
            Title = "New Item";
            Model.ItemType = "DashboardApp";
            Model.Target = string.Empty;
            Model.Url = "http://";
            Model.Icon = string.Empty;
            Model.Name = string.Empty;
            Model.Size = ItemSize.Medium;
            Model.Uid = Guid.Empty;
            Model.Monitor = true;
            IsNew = true;
        }

        if (string.IsNullOrEmpty(Model.TerminalType))
            Model.TerminalType = "SSH";

        SelectedAppName = Model.AppName;
        Loaded = true;
    }

    /// <summary>
    /// Save the editor
    /// </summary>
    /// <exception cref="Exception">throws if the item type being edited is unknown</exception>
    async Task Save()
    {
        // validate
        if (await Editor.Validate() == false)
            return;
        
        GroupItem result;
        switch (Model.ItemType)
        {
            case "DashboardApp":
                {
                    var app  = new AppItem();
                    app.Target = Model.Target;
                    app.Url = Model.Url;
                    app.Monitor = Model.Monitor;
                    app.ApiUrl = Model.ApiUrl;
                    app.AppName = Model.AppName;
                    app.DockerContainer = Model.DockerContainer;
                    app.DockerUid = Model.DockerUid == Guid.Empty ? null : Model.DockerUid;
                    app.DockerCommand = Model.DockerCommand;
                    app.SshPassword = Model.SshPassword == Globals.DUMMY_PASSWORD
                        ? Model.SshPasswordOriginal
                        : EncryptionHelper.Encrypt(Model.SshPassword);
                    app.SshServer = Model.SshServer;
                    app.SshUserName = Model.SshUserName;
                    app.Icon = Model.Icon;
                    app.Name = Model.Name;
                    app.Size = Model.Size;
                    app.Uid = Model.Uid;
                    var appPropertes = SelectedApp.Properties?.Select(x => x.Id).ToList() ?? new() { };
                    app.Properties = appPropertes?.Any() == true && Model.Properties?.Any() == true
                        ? Model.Properties.Where(x => appPropertes.Contains(x.Key))
                            .ToDictionary(x => x.Key, x => x.Value)
                        : new();
                    result = app;
                }
                break;
            case "DashboardLink":
                {
                    var link = new LinkItem();
                    link.Target = Model.Target;
                    link.Monitor = Model.Monitor;
                    link.Url = Model.Url;
                    link.Icon = Model.Icon;
                    link.Name = Model.Name;
                    link.Size = Model.Size;
                    link.Uid = Model.Uid;
                    result = link;
                }
                break;
            default:
                throw new Exception("Unknown type: " + Model.ItemType);
        }

        if (IsNew && KeepOpen) 
        {
            await OnSavedKeepOpen.InvokeAsync(result);
            // rest the model
            SelectedApp = null;
            AppSelector?.Clear();
            Model.Name = string.Empty;
            Model.AppName = string.Empty;
            Model.Uid = Guid.Empty;
            Model.Url = "https://";
            Model.Icon = string.Empty;
            Model.Properties = new();
            Model.ApiUrl = string.Empty;
            Model.SshPassword = string.Empty;
            Model.SshServer = string.Empty;
            Model.SshPasswordOriginal = string.Empty;
            Model.SshUserName = string.Empty;
            Model.DockerCommand = string.Empty;
            Model.DockerContainer = string.Empty;
            Model.DockerUid = null;
            Model.Target = string.Empty;
            Model.Size = ItemSize.Medium;
            Model.Monitor = true;
            Tabs.SelectFirstTab();
        }
        else
            await OnSaved.InvokeAsync(result);
    }


    /// <summary>
    /// Cancels the editor
    /// </summary>
    /// <returns>a task to await</returns>
    Task Cancel()
        => OnCanceled.InvokeAsync();

    /// <summary>
    /// The current app instance, used for testing the app
    /// </summary>
    private AppInstance? AppInstance;

    private bool Testing = false;
    
    /// <summary>
    /// Tests the application
    /// </summary>
    async Task TestApp()
    {
        if (AppInstance?.App?.Name != SelectedAppName)
        {
            AppInstance?.Engine?.Dispose();
            AppInstance = null;
            AppInstance = AppHeler.GetAppInstance(SelectedAppName);
            if (AppInstance == null)
                return;
        }

        Testing = true;
        await Task.Delay(1);
        try
        {
            var engine = AppInstance.Engine;

            var utils = new Utils();

            var properties = AppHeler.DecryptProperties(Model.Properties);
            var log = new List<string>();
            var args = AppHeler.GetApplicationArgs(engine,
                Model.ApiUrl?.EmptyAsNull() ?? Model.Url,
                properties, linkTarget: string.Empty, log: log);

            engine.SetValue("testArgs", args);
            engine.SetValue("testArgsUtils", utils);
            engine.Execute(@"
testArgs.Utils = testArgsUtils;
var test = instance.test(testArgs);");
            var result = engine.GetValue("test");
            result = result.UnwrapIfPromise();
            if (result == null)
                result = string.Empty;
            var str = result.ToString();
            string logStr = string.Join("\n", log);

            if (str?.ToLowerInvariant() == "true")
            {
                if(Debug && string.IsNullOrEmpty(logStr) == false)
                    ToastService.ShowSuccess(message: logStr, heading: lblTestSuccesful);
                else
                    ToastService.ShowSuccess(lblTestSuccesful);
            }
            else if (str?.ToLowerInvariant() == "false")
            {
                if (string.IsNullOrEmpty(logStr))
                    ToastService.ShowError(message: logStr, heading: lblTestFailed);
                else
                    ToastService.ShowError(lblTestFailed);
            }
            else if (str?.ToLowerInvariant() == "false")
                ToastService.ShowInfo(message: logStr, heading: str?.EmptyAsNull() ?? "Test Unknown");
            else
                ToastService.ShowInfo(str?.EmptyAsNull() ?? "Test Unknown");
        }
        catch (Exception ex)
        {
            ToastService.ShowError(ex.Message);
        }
        finally
        {
            Testing = false;
            this.StateHasChanged();
        }
    }

    public void Dispose()
    {
        AppSelector?.Dispose();
        AppInstance?.Engine?.Dispose();
        AppInstance = null;
    }
}

/// <summary>
/// The model for the Group Item Editor
/// </summary>
class GroupItemEditorModel : GroupItem
{
    /// <summary>
    /// Gets the type being edited
    /// </summary>
    public override string Type => ItemType;
    /// <summary>
    /// Gets or sets the type being edited
    /// </summary>
    public string ItemType { get; set; }
    /// <summary>
    /// Gets or sets the URL of the item being edited
    /// </summary>
    public string Url { get; set; }
    
    /// <summary>
    /// Gets or sets the terminal type
    /// </summary>
    public string TerminalType { get; set; }

    /// <summary>
    /// Gets or sets the API URL (used by smart apps)
    /// </summary>
    public string ApiUrl { get; set; }

    /// <summary>
    /// Gets or sets the size as a string
    /// </summary>
    public string SizeString
    {
        get => Size.ToString();
        set => Size = Enum.Parse<ItemSize>(value);
    }

    /// <summary>
    /// Gets or sets the name of the application this is an instance of
    /// </summary>
    public string AppName { get; set; }
    
    /// <summary>
    /// Gets or sets the target to open this item
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// Gets or sets the SSH Server for this item
    /// </summary>
    public string SshServer { get; set; }
    /// <summary>
    /// Gets or sets the SSH username for this item
    /// </summary>
    public string SshUserName { get; set; }
    /// <summary>
    /// Gets or sets the SSH password for this item
    /// </summary>
    public string SshPassword { get; set; }
    /// <summary>
    /// Gets or sets the original SSH password
    /// </summary>
    public string SshPasswordOriginal { get; set; }
    /// <summary>
    /// Gets or sets the Docker UID for this item
    /// </summary>
    public Guid? DockerUid { get; set; }
    /// <summary>
    /// Gets or sets the Docker container for this item
    /// </summary>
    public string DockerContainer { get; set; }
    /// <summary>
    /// Gets or sets the Docker command for this item
    /// </summary>
    public string DockerCommand { get; set; }

    /// <summary>
    /// Gets or sets any additional smart app properties for this item
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Gets a smart app property value
    /// </summary>
    /// <param name="prop">the property to lookup</param>
    /// <returns>the value, or the FenrusAppProperty.DefaultValue if not set</returns>
    public object? GetValue(FenrusAppProperty prop)
    {
        if (Properties.ContainsKey(prop.Id) == false)
        {
            if (prop.DefaultValue != null)
            {
                if (prop.DefaultValue is JsonElement je)
                {
                    // in case its a json element value, we need to convert it
                    prop.DefaultValue = je.ValueKind == JsonValueKind.String ? je.GetString() :
                        je.ValueKind == JsonValueKind.False ? false :
                        je.ValueKind == JsonValueKind.True ? true :
                        je.ValueKind == JsonValueKind.Number ? je.GetInt32() :
                        je.ValueKind == JsonValueKind.Null ? null :
                        null;
                }
                Properties.Add(prop.Id, prop.DefaultValue);
            }

            return prop?.DefaultValue;
        }

        if (prop.Type == AppPropertyType.Password)
        {
            if(string.IsNullOrEmpty(Properties[prop.Id]?.ToString()))
                return string.Empty;
            return Globals.DUMMY_PASSWORD;
        }

        return Properties[prop.Id];
    }

    /// <summary>
    /// Sets a smart app property value
    /// </summary>
    /// <param name="prop">the property to set</param>
    /// <param name="value">the value being set</param>
    public void SetValue(FenrusAppProperty prop, object value)
    {
        if (prop.Type == AppPropertyType.Password)
        {
            if (value?.ToString() == Globals.DUMMY_PASSWORD)
                return; // dont update
            value = EncryptionHelper.Encrypt(value?.ToString() ?? string.Empty);
        }
        
        if (Properties.ContainsKey(prop.Id))
            Properties[prop.Id] = value;
        else
            Properties.Add(prop.Id, value);
    }
}