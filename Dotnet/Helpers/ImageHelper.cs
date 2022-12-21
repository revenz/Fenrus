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
        var image = ImageFromBase64(base64);
        if (image.Data?.Any() != true)
            return string.Empty;

        using var db = DbHelper.GetDb();
        string id = Guid.NewGuid().ToString();
        db.FileStorage.Upload("db:/image/" + id, id + "." + image.Extension, new MemoryStream(image.Data));
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