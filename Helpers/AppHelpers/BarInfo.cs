namespace Fenrus.Helpers.AppHelpers;

/// <summary>
/// Bar Info helper
/// </summary>
public class BarInfo
{
    /// <summary>
    /// Generates the bar info response
    /// </summary>
    public static string Generate(Utils utils, BarInfoItem[] items)
    {
        var html = ":bar-info:";
        foreach(var item in items)
        {
            if(item.Percent != null)
            {
                html += $@"<div class=""bar-info"" {(string.IsNullOrEmpty(item.ToolTip) ? ("title=\"" + utils.htmlEncode(item.ToolTip) + '"') : string.Empty)}>" +
                            (string.IsNullOrEmpty(item.Icon) ? $")<div class=\"bar-icon\"><img src=\"{item.Icon}\" /></div>" : "") +
                            "<div class=\"bar\">" +
                                $"<div class=\"fill\" style=\"width:{item.Percent}%\"></div>" +
                                "<div class=\"labels\">" + 
                                    $"<span class=\"info-label\">{utils.htmlEncode(item.Label)}</span>" +
                                    $"<span class=\"fill-label\">{item.Percent.Value:#.#}%</span>" +
                                "</div>" + 
                            "</div>" + 
                        "</div>";
            }
            else if(item.Value != null)
            {
                html += "<div class=\"bar-info-label-value\">" +                         
                        $"<span class=\"label\">{item.Label}</span>" +
                        $"<span class=\"value\">{item.Value}</span>" +
                        "</div>";
            }
        }        
        return html;
    }
    
    /// <summary>
    /// Item in the bar info
    /// </summary>
    public class BarInfoItem
    {
        /// <summary>
        /// Gets or sets the label
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// Gets or sets the percent
        /// </summary>
        public double? Percent { get; set; }
        /// <summary>
        /// Gets or sets the icon
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// Gets or sets a tootlip
        /// </summary>
        public string ToolTip { get; set; }
        /// <summary>
        /// Gets or sets the value to show
        /// </summary>
        public object Value { get; set; }
    }
}