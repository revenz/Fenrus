using Microsoft.AspNetCore.Components;

namespace Fenrus.Components.Inputs;

/// <summary>
/// Wrapper for inputs
/// </summary>
public partial class InputWrapper
{
    [Parameter] public IInput Input { get; set; }

    [Parameter] public RenderFragment ChildContent { get; set; }
    private string HelpHtml = string.Empty;
    private string CurrentHelpText;
    
        
    protected override void OnInitialized()
    {
        InitHelpText();
    }
    protected override void OnParametersSet()
    {
        if (CurrentHelpText != Input?.Help)
        {
            InitHelpText();
            this.StateHasChanged();
        }
    }
    private void InitHelpText()
    {
        CurrentHelpText = Input?.Help;
        if (string.IsNullOrEmpty(Input?.Help))
        {
            this.HelpHtml = string.Empty;
        }
        else
        {
            string help = Markdig.Markdown.ToHtml(Input.Help).Trim();
            if (help.StartsWith("<p>") && help.EndsWith("</p>"))
                help = help[3..^4].Trim();
            help = help.Replace("<a ", "<a rel=\"noreferrer\" target=\"_blank\" ");
            this.HelpHtml = help;
        }
    }
}