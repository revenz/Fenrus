using System.Runtime.ConstrainedExecution;
using Fenrus.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Fenrus.Components;

/// <summary>
/// JavaScript version of Fenrus ColorPicker
/// All events for value changes call javascript function
/// This is used on the main dashboard screen in the panel settings
/// </summary>
public partial class JavaScriptColorPicker
{
    /// <summary>
    /// The unique id of this instance of this control
    /// </summary>
    private string Uid = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets or sets if this is checked
    /// </summary>
    [Parameter] public string Value { get; set; }
    
    /// <summary>
    /// Gets or sets the code to run when the value changes
    /// </summary>
    [Parameter] public string Code { get; set; }
    
    
    /// <summary>
    /// Gets or sets the page helper
    /// </summary>
    [Parameter] public PageHelper PageHelper { get; set; }
    
    protected override void OnInitialized()
    {
        PageHelper.ScriptBlocks.Add($"new JSColor(document.getElementById('{this.Uid}-input'), " +
                                    $"{{ container: document.getElementById('{this.Uid}'), value: '{this.Value}', mode: 'HVS', shadow: false, controlBorderColor: 'var(--input-border)', borderColor: 'var(--input-border)', borderRadius: 0, alphaChannel: false, onChange: function() {{ " +
                                    " let color = this.toHEXString();" + 
                                    $" {Code}" +
                                    $"}} " +
                                    $"}});");
    }
}