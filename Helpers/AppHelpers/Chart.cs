namespace Fenrus.Helpers.AppHelpers;

/// <summary>
/// Chart helper
/// </summary>
public class Chart
{
    public string Line(object args)
        => "chart:line:" + JsonSerializer.Serialize(args);   
}

/// <summary>
/// Arguments for charts
/// </summary>
public class LineChartArgs
{
    /// <summary>
    /// Gets or sets the chart title
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// Gets or sets any labels for the chart
    /// </summary>
    public List<string> Labels { get; set; }
    /// <summary>
    /// Gets or sets the chart data
    /// </summary>
    public double[] Data { get; set; }

    /// <summary>
    /// Gets or sets the Minimum for the chart
    /// </summary>
    public int? Min { get; set; }
    /// <summary>
    /// Gets or sets the Maximum for the chart
    /// </summary>
    public int? Max { get; set; }
}