using Humanizer;

namespace Fenrus.Helpers.AppHelpers;

/// <summary>
/// Humanizer helper class that is passed to Jint smart apps
/// </summary>
public class Humanizer
{
    /// <summary>
    /// Converts milliseconds into a human readable string
    /// </summary>
    /// <param name="milliseconds">the milliseconds</param>
    /// <returns>a human readable string</returns>
    public string Milliseconds(long milliseconds, HumanizerOptions options)
        => options?.Precision == null
            ? TimeSpan.FromMilliseconds(milliseconds).Humanize()
            : TimeSpan.FromMilliseconds(milliseconds).Humanize(options.Precision.Value);
}

/// <summary>
/// Options for the humanizer class
/// </summary>
public class HumanizerOptions
{
    /// <summary>
    /// Gets or sets the precision to use
    /// </summary>
    public int? Precision { get; set; }
}