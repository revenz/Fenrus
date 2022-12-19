namespace Fenrus;

/// <summary>
/// Extensions Methods
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// If the string is empty or white space, then returns null, otherwise the strings value
    /// </summary>
    /// <param name="str">the string</param>
    /// <returns>null if empty</returns>
    public static string? EmptyAsNull(this string str)
         => string.IsNullOrWhiteSpace(str) ? null : str;
    
}