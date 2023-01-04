using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.Inputs;

/// <summary>
/// Input text
/// </summary>
public partial class InputText : Input<string>
{
    /// <summary>
    /// Gets or sets if this field is required
    /// </summary>
    [Parameter] public bool Required { get; set; }

    private string _Pattern;
    /// <summary>
    /// Gets or sets a regular expression pattern this value must match if required
    /// </summary>
    [Parameter]
    public string Pattern
    {
        get => _Pattern;
        set
        {
            _Pattern = value;
            PatternRegex = string.IsNullOrWhiteSpace(value) ? null : new Regex(value);
        }
    }

    private Regex? PatternRegex;


    /// <summary>
    /// Validates the input
    /// </summary>
    /// <returns>true if the input is valid, otherwise false</returns>
    public override Task<bool> Validate()
    {
        this.ErrorMessage = string.Empty;
        if (Required)
        {
            if (string.IsNullOrWhiteSpace(this.Value))
            {
                this.ErrorMessage = "Required";
                return Task.FromResult(false);
            }
        }

        if (string.IsNullOrWhiteSpace(Value) == false && Required && PatternRegex != null)
        {
            if (PatternRegex.IsMatch(Value) == false)
            {
                this.ErrorMessage = "Invalid input";
                return Task.FromResult(false);
            }
        }

        return base.Validate();
    }
}