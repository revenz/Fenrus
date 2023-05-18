using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.JSInterop;

namespace Fenrus.Components.Inputs;

/// <summary>
/// Input image
/// </summary>
public partial class InputImage : Input<string>
{
    /// <summary>
    /// Gets or sets the initial image to show in the preview
    /// </summary>
    [Parameter] public string InitialImage { get; set; }
    
    /// <summary>
    /// Gets or sets if the reset button should be shown
    /// </summary>
    [Parameter] public bool Reset { get; set; }
    
    /// <summary>
    /// Gets or sets a custom preview template to show instead of the default preview
    /// </summary>
    [Parameter] public RenderFragment<string> PreviewTemplate { get; set; }

    private async Task ImageChosen(ChangeEventArgs e)
    {
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if(firstRender)
            _ = jsRuntime.InvokeVoidAsync("CreateImagePicker", 
                DotNetObjectReference.Create(this), 
                this.Uid.ToString());
    }
    
    [JSInvokable("updateValue")]
    public Task UpdateValue(string value)
    {
        this.Value = value;
        StateHasChanged(); 
        return Task.CompletedTask;
    }

    private async Task ResetImage()
    {
        this.Value = "";
        this.InitialImage = "";
    }

    private async Task OpenIconPicker()
    {
        var result = await Dialogs.IconPickerDialog.Show();
        if (string.IsNullOrEmpty(result))
            return;
        var app = AppService.GetByName(result);
        if (app == null)
            return;
        this.Value = $"/apps/{app.Name}/{app.Icon}";
        StateHasChanged();
    }
}