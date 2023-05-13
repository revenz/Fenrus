namespace Fenrus.Services;

/// <summary>
/// A mail attachment
/// </summary>
public class MailAttachment
{
    /// <summary>
    /// Gets the name of this attachment
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// Gets the binary data for this attachment
    /// </summary>
    public byte[] Data { get; init; }
}