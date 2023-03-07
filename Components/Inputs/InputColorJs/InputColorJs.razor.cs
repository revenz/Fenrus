using System.Runtime.InteropServices;
using Fenrus.Components.Inputs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.Win32.SafeHandles;

namespace Fenrus.Components.Inputs;

/// <summary>
/// Input Color
/// </summary>
public partial class InputColorJs : Input<string>
{
    [Inject] private IJSRuntime jsRuntime { get; set; }


    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender)
            jsRuntime.InvokeVoidAsync($"CreateColorPicker", this.Uid + "-input");
    }
    
    [JSInvokable("updateValue")]
    public Task UpdateValue(string value)
    {
        this.Value = value;
        StateHasChanged(); 
        return Task.CompletedTask;
    }
}