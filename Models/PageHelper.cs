namespace Fenrus.Models;

/// <summary>
/// Helper passed along to pages to allow razor components to register javascript etc
/// </summary>
public class PageHelper
{
    /// <summary>
    /// Gets or sets the script blocks to execute on the client side
    /// </summary>
    public List<string> ScriptBlocks { get; private set; } = new();
    
    /// <summary>
    /// Gets or sets the translater to use
    /// </summary>
    public Translater Translater { get; init; }
    
    public void RegisterScriptBlock(string script)
    {
        ScriptBlocks.Add(script);
    }

    /// <summary>
    /// Constructs a page helper
    /// </summary>
    /// <param name="translater">the translater to use for translations</param>
    public PageHelper(Translater translater)
    {
        this.Translater = translater;
    }
}