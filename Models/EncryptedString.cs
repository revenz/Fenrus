namespace Fenrus.Models;

/// <summary>
/// Encrypted string used for LiteDB
/// </summary>
public class EncryptedString
{
    /// <summary>
    /// Gets or sets the value of the string
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The string value
    /// </summary>
    /// <returns>the string value</returns>
    public override string ToString()
        => this.Value;

    /// <summary>
    /// Converts a encrypted string to a string
    /// </summary>
    /// <param name="value">the encrypted string</param>
    /// <returns>the string</returns>
    public static implicit operator string(EncryptedString value)
        => value?.Value ?? string.Empty;

    /// <summary>
    /// Converts a string to an encrypted string
    /// </summary>
    /// <param name="value">the string value</param>
    /// <returns>the encrypted string</returns>
    public static explicit operator EncryptedString(string value)
        => new () { Value = value };
}