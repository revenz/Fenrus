namespace Fenrus.Models;

/// <summary>
/// Helper passed along to pages to allow razor components to register javascript etc
/// </summary>
public class PageHelper
{
    public List<string> ScriptBlocks { get; private set; } = new();

    public void RegisterScriptBlock(string script)
    {
        ScriptBlocks.Add(script);
    }
}