using System.Text.RegularExpressions;

namespace Fenrus.Helpers;

/// <summary>
/// Helper for images
/// </summary>
public class ImageHelper
{
    /// <summary>
    /// Gets an image from a base64 string
    /// </summary>
    /// <param name="base64">the base 64 string</param>
    /// <returns>the image data</returns>
    public static (byte[] Data, string Extension) ImageFromBase64(string base64)
    {
        // data:image/jpeg;base64,
        if (base64?.StartsWith("data:image/") != true)
            return (new byte[] { }, string.Empty); // not valid base64 image
        string b64 = base64["data:image/".Length..];
        string extension = b64.Substring(0, b64.IndexOf(";", StringComparison.Ordinal)).ToLower();
        if (extension == "jpeg")
            extension = "jpg";
        b64 = b64.Substring(b64.IndexOf(",", StringComparison.Ordinal) + 1);
        var data = Convert.FromBase64String(b64);
        return (data, extension);

    }

    /// <summary>
    /// Saves an image into the database and returns its ID
    /// </summary>
    /// <param name="base64">the base64 image</param>
    /// <returns>the image ID, or empty if no image was passed in</returns>
    public static string SaveImageFromBase64(string base64)
    {
        if (string.IsNullOrEmpty(base64))
            return string.Empty;
        
        if (base64.StartsWith("data:image/") == false)
            return SaveImageFromBase64Data(base64);
        
        var image = ImageFromBase64(base64);
        if (image.Data?.Any() != true)
            return string.Empty;

        using var db = DbHelper.GetDb();
        string id = Guid.NewGuid().ToString();
        db.FileStorage.Upload("db:/image/" + id, id + "." + image.Extension, new MemoryStream(image.Data));
        return "db:/image/" + id;
    }

    /// <summary>
    /// Saves an image into the database and returns its ID
    /// </summary>
    /// <param name="base64">the base64 data byte array</param>
    /// <returns>the image ID, or empty if no image was passed in</returns>
    private static string SaveImageFromBase64Data(string base64)
    {
        if (string.IsNullOrEmpty(base64))
            return string.Empty;
        
        var data = Convert.FromBase64String(base64);

        using var db = DbHelper.GetDb();
        string id = Guid.NewGuid().ToString();
        db.FileStorage.Upload("db:/image/" + id, id, new MemoryStream(data));
        return "db:/image/" + id;
    }
    
    /// <summary>
    /// Saves an image into the database and returns its ID
    /// </summary>
    /// <param name="data">the image data</param>
    /// <param name="extension">the image extension</param>
    /// <param name="uid">[Optional] UID of the image to save</param>
    /// <returns>the image ID, or empty if no image was passed in</returns>
    public static string SaveImage(byte[] data, string extension, Guid? uid = null)
    {
        if (Regex.IsMatch(extension, "^[a-zA-Z]+$") == false)
            return string.Empty; // disallowed extension, we dont want them use .. or anything invalid here
        if (data?.Any() != true)
            return string.Empty;
        
        using var db = DbHelper.GetDb();
        string id = uid == null || uid == Guid.Empty ? Guid.NewGuid().ToString() : uid.Value.ToString();
        db.FileStorage.Upload("db:/image/" + id, id + "." + extension, new MemoryStream(data));
        return "db:/image/" + id;
    }

    /// <summary>
    /// Deletes an image from the database
    /// </summary>
    /// <param name="id">the image id</param>
    public static void DeleteImage(string id)
    {
        using var db = DbHelper.GetDb();
        id = "db:/image/" + id;
        db.FileStorage.Delete(id);
    }
}