using System.Runtime.ConstrainedExecution;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// JavaScript version of Fenrus Select
/// All events for value changes call javascript function
/// This is used on the main dashboard screen in the panel settings
/// </summary>
public partial class JavaScriptSelect
{
    private string _Id = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets or sets the ID of the DOM element
    /// </summary>
    [Parameter]
    public string Id
    {
        get => _Id;
        set => _Id = value?.EmptyAsNull() ?? Guid.NewGuid().ToString();
    }
    
    /// <summary>
    /// Gets or sets if this is checked
    /// </summary>
    [Parameter] public string Value { get; set; }
    
    /// <summary>
    /// Gets or sets the options
    /// </summary>
    [Parameter] public string[] Options { get; set; }

    /// <summary>
    /// Gets or sets the values for match the options
    /// </summary>
    [Parameter] public string[]? Labels { get; set; }
    
    /// <summary>
    /// Gets or sets the code to run when the value changes
    /// </summary>
    [Parameter] public string Code { get; set; }


    private string InitialString;
    
    protected override void OnInitialized()
    {
        var index = Array.FindIndex(Options, x => x == Value);
        if (index >= 0)
        {
            InitialString = (Labels != null && Labels.Length > index) ? Labels[index] : Value;
        }
        else
        {
            InitialString = "Please Select";
        }
    }
}