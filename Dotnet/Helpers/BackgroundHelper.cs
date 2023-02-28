namespace Fenrus.Helpers;

/// <summary>
/// Helper class for backgrounds
/// </summary>
public class BackgroundHelper
{
    
    /// <summary>
    /// Gets the names for the background
    /// </summary>
    /// <returns>the background names</returns>
    public static string[] GetBackgroundNames()
    {
        return new string[]
        {
            "Default",
            "Vanta Birds",
            "Vanta Cells",
            //"Vanta Clouds",
            //"Vanta Clouds 2",
            "Vanta Dots",
            "Vanta Fog",
            "Vanta Waves",
        };
    }

    /// <summary>
    /// Gets the values for the background
    /// </summary>
    /// <returns>the background values</returns>
    public static string[] GetBackgroundValues()
    {
        return new string[]
        {
            "default.js",
            "vanta.birds.js",
            "vanta.cells.js",
            //"vanta.clouds.js",
            //"vanta.clouds2.js",
            "vanta.dots.js",
            "vanta.fog.js",
            "vanta.waves.js"
        };
    }
}