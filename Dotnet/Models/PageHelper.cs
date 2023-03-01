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
    
    public void RegisterScriptBlock(string script)
    {
        ScriptBlocks.Add(script);
    }
}