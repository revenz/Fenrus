using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Fenrus.Components.Inputs;

public interface IInput
{
    string Label { get; set; }
    string Help { get; set; }
    string Placeholder { get; set; }
    string ErrorMessage { get; set; }
    bool HideLabel { get; set; }
    bool Disabled { get; set; }
    bool Visible { get; set; }

    void Dispose();

    bool Focus();
    Task<bool> Validate();

}


public abstract class Input<T> : ComponentBase, IInput, IDisposable
{
    [Inject] protected IJSRuntime jsRuntime { get; set; }
    protected string Uid = System.Guid.NewGuid().ToString();
    private string _Help;
    public EventHandler<bool> ValidStateChanged { get; set; }

    public string Suffix { get; set; }
    public string Prefix { get; set; }
    
    [Parameter] public EventCallback OnSubmit { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    [Parameter]
    public bool HideLabel { get; set; }

    [Parameter] public string Label { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }
    
    [CascadingParameter] public EditorForm Editor { get; set; }

    [Parameter]
    public bool Disabled { get; set; }
    public bool Visible { get; set; }

    [Parameter]
    public string Help 
    { 
        get => _Help;
        set => _Help = value ?? string.Empty;
    } 
    public string _Placeholder;

    [Parameter]
    public string Placeholder
    {
        get => _Placeholder;
        set { _Placeholder = value ?? ""; }
    }


    private string _ErrorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _ErrorMessage;
        set
        {
            _ErrorMessage = value;
        }
    }

    /// <summary>
    /// Gets the text to show as the placeholder
    /// </summary>
    /// <returns>the text to show as the placeholder</returns>
    protected string GetPlaceholder()
    {
        if (string.IsNullOrEmpty(this.Placeholder) == false)
            return this.Placeholder;
        if (this.HideLabel)
            return this.Label;
        return string.Empty;
    }

    protected T _Value;
    [Parameter]
    public T Value
    {
        get => _Value;
        set
        {
            if (Disposed) return;

            if (_Value == null && value == null)
                return;

            if (_Value != null && value != null && _Value.Equals(value)) return;

            bool areEqual = System.Text.Json.JsonSerializer.Serialize(_Value) ==
                            System.Text.Json.JsonSerializer.Serialize(value);
            if (areEqual ==
                false) // for lists/arrays if they haven't really changed, empty to empty, dont clear validation
                ErrorMessage = ""; // clear the error

            _Value = value;
            ValueUpdated();
            ValueChanged.InvokeAsync(value);
        }
    }

    protected virtual void ValueUpdated() { }

    [Parameter]
    public EventCallback<T> ValueChanged { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if(this.Editor != null)
            this.Editor.RegisterControl(this);
        this.Visible = true;

    }

    public virtual bool Focus() => false;

    protected bool FocusUid()
    {
        if (Disposed) return false;
        _ = jsRuntime.InvokeVoidAsync("eval", $"document.getElementById('{Uid}').focus()");
        return true;
    }

    private bool Disposed = false;
    public virtual void Dispose()
    {
        this.Editor?.UnregisterControl(this);
        Disposed = true;
    }

    /// <summary>
    /// Validates the input
    /// </summary>
    /// <returns>true if the input is valid, otherwise false</returns>
    public virtual Task<bool> Validate() => Task.FromResult(true);
}