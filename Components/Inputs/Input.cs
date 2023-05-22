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
    bool Dirty { get; set; }
    
    string TranslatedLabel { get; }

    void Dispose();

    bool Focus();
    Task<bool> Validate();

}


public abstract class Input<T> : ComponentBase, IInput, IDisposable
{
    /// <summary>
    /// Gets or sets the JavaScript runtime
    /// </summary>
    [Inject] protected IJSRuntime jsRuntime { get; set; }
    
    protected string Uid = System.Guid.NewGuid().ToString();
    private string _Help;
    
    /// <summary>
    /// Gets or sets if this control has been rendered yet
    /// </summary>
    private bool Rendered { get; set; }
    
    /// <summary>
    /// Gets or sets the valid state change event
    /// </summary>
    public EventHandler<bool> ValidStateChanged { get; set; }

    /// <summary>
    /// Gets or sets a suffix  to show in this control input
    /// </summary>
    public string Suffix { get; set; }
    /// <summary>
    /// Gets or sets a prefix to show in this control input
    /// </summary>
    public string Prefix { get; set; }
    
    /// <summary>
    /// Gets or sets the page this control is on, used for translations
    /// </summary>
    [Parameter] public string Page { get; set; }
    
    /// <summary>
    /// Gets or sets the event to call when this is submitted
    /// </summary>
    [Parameter] public EventCallback OnSubmit { get; set; }
    /// <summary>
    /// Gets or sets the event to call when this is closed
    /// </summary>
    [Parameter] public EventCallback OnClose { get; set; }

    /// <summary>
    /// Gets or sets if the label for this should be hidden
    /// </summary>
    [Parameter]
    public bool HideLabel { get; set; }

    private string _Label;
    private string _OriginalLabel;
    
    /// <summary>
    /// Gets or sets the label for this control
    /// </summary>
    [Parameter]
    public string Label
    {
        get => _Label;
        set
        {
            _Label = value;
            _OriginalLabel = value;
        }
    }

    private string _TranslatedLabel;
    /// <summary>
    /// Gets the translated label
    /// </summary>
    public string TranslatedLabel => _TranslatedLabel?.EmptyAsNull() ?? this.Label;

    /// <summary>
    /// Gets or sets if this control is readonly
    /// </summary>
    [Parameter]
    public bool ReadOnly { get; set; }
    
    /// <summary>
    /// Gets or sets the editor to which this control belongs to
    /// </summary>
    [CascadingParameter] public EditorForm Editor { get; set; }

    /// <summary>
    /// Gets or sets if this control is disabled
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }
    /// <summary>
    /// Gets or sets if this control is visible
    /// </summary>
    public bool Visible { get; set; }
    /// <summary>
    /// Gets or sets if this control is dirty
    /// </summary>
    public bool Dirty { get; set; }

    /// <summary>
    /// Gets the translator to use for this page
    /// </summary>
    [CascadingParameter]
    protected Translator Translator { get; set; }

    /// <summary>
    /// Gets or sets the help for this control
    /// </summary>
    [Parameter]
    public string Help 
    { 
        get => _Help;
        set => _Help = value ?? string.Empty;
    } 
    public string _Placeholder;

    
    /// <summary>
    /// Gets or sets the placeholder for this control
    /// </summary>
    [Parameter]
    public string Placeholder
    {
        get => _Placeholder;
        set => _Placeholder = value ?? "";
    }


    /// <summary>
    /// Gets or sets a error message to show for this control
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

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
    /// <summary>
    /// Gets or sets the value bound to this control
    /// </summary>
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
 
            bool areEqual = JsonSerializer.Serialize(_Value) == JsonSerializer.Serialize(value);
            if (areEqual == false) // for lists/arrays if they haven't really changed, empty to empty, dont clear validation
                ErrorMessage = string.Empty; // clear the error

            _Value = value;
            ValueUpdated();
            ValueChanged.InvokeAsync(value);
            if(Rendered)
                Dirty = true;
        }
    }

    protected virtual void ValueUpdated() { }

    /// <summary>
    /// Gets or sets the vent callback to invoke when the value changes
    /// </summary>
    [Parameter]
    public EventCallback<T> ValueChanged { get; set; }

    /// <summary>
    /// Initializes the control
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if(this.Editor != null)
            this.Editor.RegisterControl(this);

        if (string.IsNullOrEmpty(this.Page) == false)
            UseTranslationLabel($"Pages.{Page}.Fields");
        
        this.Visible = true;

    }

    /// <summary>
    /// If the page is set for the control, automatically lookup the translations for it 
    /// </summary>
    /// <param name="prefix">the prefix to lookup</param>
    private void UseTranslationLabel(string prefix)
    {
        string field = this._OriginalLabel;
        if(string.IsNullOrEmpty(_Help))
            _Help = Translator.Instant($"{prefix}.{field}-Help");
        if(string.IsNullOrEmpty(_Placeholder))
            _Placeholder = Translator.Instant($"{prefix}.{field}-Placeholder");
        _TranslatedLabel = Translator.Instant($"Pages.{Page}.Fields.{Label}");
        _Placeholder = _Placeholder.EmptyAsNull() ?? _Label;
    }

    /// <summary>
    /// Focuses the input
    /// </summary>
    /// <returns>true if focused, otherwise false</returns>
    public virtual bool Focus() => false;

    /// <summary>
    /// Focuses the input using its UID
    /// </summary>
    /// <returns>true if successful, otherwise false</returns>
    protected bool FocusUid()
    {
        if (Disposed) return false;
        _ = jsRuntime.InvokeVoidAsync("eval", $"document.getElementById('{Uid}').focus()");
        return true;
    }

    private bool Disposed = false;
    
    
    /// <summary>
    /// Disposes of the control
    /// </summary>
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

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            this.Rendered = true;
        return base.OnAfterRenderAsync(firstRender);
    }
}