using MailKit;

namespace Fenrus.Models;

/// <summary>
/// A user file
/// </summary>
public class UserFile
{
    /// <summary>
    /// Gets or sets the full path of the file
    /// </summary>
    public string FullPath { get; set; }
    
    /// <summary>
    /// Gets or sets the short name of the file
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets extension of the file
    /// </summary>
    public string Extension { get; set; }
    
    /// <summary>
    /// Gets or sets the date the note was taken
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Gets or sets the mime type of the file
    /// </summary>
    public string MimeType { get; set; }
    
    /// <summary>
    /// Gets or sets the file size
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets the icon to use for this file type
    /// </summary>
    public string Icon
    {
        get
        {
            if (string.IsNullOrEmpty(MimeType))
                return "fa-solid fa-file";
            if (MimeType == "folder")
                return "fa-solid fa-folder";
            if (MimeType.Contains("image/"))
                return "fa-solid fa-file-image";
            if (MimeType.Contains("excel") || MimeType.Contains("xls"))
                return "fa-solid fa-excel";
            if (MimeType.Contains("word") || MimeType.Contains("doc"))
                return "fa-solid file-word";
            if (MimeType.Contains("csv"))
                return "fa-solid fa-file-csv";
            if (MimeType.Contains("powerpoint"))
                return "fa-solid fa-powerpoint";
            if (MimeType.Contains("pdf"))
                return "fa-solid file-pdf";
            if (MimeType.Contains("text"))
                return "fa-solid fa-file-lines";
            if (MimeType.Contains("zip") || MimeType.Contains("archive"))
                return "fa-solid fa-file-archive";
            if (MimeType.Contains("video"))
                return "fa-solid fa-file-video";
            if (MimeType.Contains("music"))
                return "fa-solid fa-file-audio";
            
            if (MimeType.Contains("code") || new [] {"json", "html", "xml", "js", "cs", "cpp", "js", "ts"}.Contains(this.Extension))
                return "fa-solid fa-file-code";
            
            return "fa-solid fa-file";
        }
    }
}