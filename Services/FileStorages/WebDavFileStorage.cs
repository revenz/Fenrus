using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Fenrus.Models;

namespace Fenrus.Services.FileStorages;

/// <summary>
/// File Storage using WebDAV
/// </summary>
public class WebDavFileStorage : IFileStorage
{
    private string provider;
    private string baseUrl;
    private string serverUrl;
    private string username;
    private string password;
    private HttpClient client;

    public WebDavFileStorage(string provider, string url, string username, string password)
    {
        this.baseUrl = url;
        this.provider = provider;
        this.username = username;
        this.password = password;
        this.serverUrl = GetServerUrl(provider, url, username, password);
        this.client = GetClient(username, password);
    }

    private static HttpClient GetClient(string username, string password)
    {
        var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        return client;
    }


    /// <summary>
    /// Gets all the UID for files for a user
    /// </summary>
    /// <param name="path">the users folder path to get</param>
    /// <returns>all the files UIDs for the user</returns>
    public async Task<List<UserFile>> GetAll(string path)
    {
        string folder = path;
        if (!String.IsNullOrEmpty(folder))
        {
            folder = folder.TrimStart('/');
            folder = folder.TrimEnd('/');
        }

        string url = serverUrl;
        if (!String.IsNullOrEmpty(folder))
        {
            url += "/" + folder;
        }

        var propfindContent = new StringContent(
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?><D:propfind xmlns:D=\"DAV:\"><D:prop><D:displayname/><D:getlastmodified/><D:getcontenttype/><D:getcontentlength/></D:prop></D:propfind>",
            Encoding.UTF8, "text/xml");
        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod("PROPFIND"), url)
            { Content = propfindContent });

        var xmlResponse = await response.Content.ReadAsStringAsync();
        var doc = new XmlDocument();
        doc.LoadXml(xmlResponse);
        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("D", "DAV:");
        var nodes = doc.DocumentElement.SelectNodes("//D:response", nsmgr);

        var files = new List<UserFile>();
        foreach (XmlNode node in nodes)
        {
            var displayNameNode = node.SelectSingleNode("D:propstat/D:prop/D:displayname", nsmgr);
            var contentTypeNode = node.SelectSingleNode("D:propstat/D:prop/D:getcontenttype", nsmgr);
            var contentLengthNode = node.SelectSingleNode("D:propstat/D:prop/D:getcontentlength", nsmgr);
            var lastModifiedNode = node.SelectSingleNode("D:propstat/D:prop/D:getlastmodified", nsmgr);

            if (displayNameNode == null || string.IsNullOrEmpty(displayNameNode.InnerText))
                continue;
            
            var fullPath = CleanFullPath(node.SelectSingleNode("D:href", nsmgr)?.InnerText);
            if (string.IsNullOrWhiteSpace(fullPath) || fullPath == path)
                continue;
            
            var name = displayNameNode.InnerText;
            var extension = Path.GetExtension(name);
            var createdUtc = DateTime.MinValue;
            var mimeType = contentTypeNode?.InnerText ?? "";
            long.TryParse(contentLengthNode?.InnerText ?? string.Empty, out var size);
            var isFolder = fullPath.EndsWith("/") || fullPath.EndsWith("\\");
            if (isFolder)
                mimeType = "folder";

            if (lastModifiedNode != null && string.IsNullOrEmpty(lastModifiedNode.InnerText) == false)
                DateTime.TryParse(lastModifiedNode.InnerText, out createdUtc);

            files.Add(new UserFile()
            {
                FullPath = fullPath,
                Name = name,
                Extension = extension,
                Created = createdUtc,
                MimeType = mimeType,
                Size = size
            });
            
        }

        return files.OrderBy(x => x.MimeType == "folder" ? 0 : 1).ThenBy(x => x.Name.ToLowerInvariant()).ToList();
    }


    /// <summary>
    /// Gets the file data by its path
    /// </summary>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>the file</returns>
    public async Task<FileData?> GetFileData(string fullPath)
    {
        if(string.IsNullOrWhiteSpace(fullPath))
            return null;

        string url = serverUrl + "/" + fullPath + "?x=thumbnail";
        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode == false)
            return null;
        var mimeType = response.Content.Headers?.ContentType?.MediaType ?? string.Empty;
        var stream = await response.Content.ReadAsStreamAsync();
        return new()
        {
            Data = stream,
            MimeType = mimeType,
            Filename = fullPath[(fullPath.LastIndexOf("/") + 1)..]
        };
    }

    /// <summary>
    /// Gets a thumbnail for an image
    /// </summary>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>the file data</returns>
    public async Task<FileData?> GetThumbnail(string fullPath)
    {
        if(string.IsNullOrWhiteSpace(fullPath))
            return null;

        string url = GetThumnailUrl(fullPath);
        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode == false)
            return await GetFileData(fullPath);
        var mimeType = response.Content.Headers?.ContentType?.MediaType ?? string.Empty;
        var stream = await response.Content.ReadAsStreamAsync();
        return new()
        {
            Data = stream,
            MimeType = mimeType,
            Filename = fullPath[(fullPath.LastIndexOf("/") + 1)..]
        };
    }

    /// <summary>
    /// Adds a file item to the database for the user
    /// </summary>
    /// <param name="path">the path to upload this file to</param>
    /// <param name="filename">the short filename of the file being added</param>
    /// <param name="formFile">the form file being added</param>
    /// <returns>the id of the newly created file</returns>
    public async Task<UserFile?> Add(string path, string filename, IFormFile formFile)
    {
        // Construct the full URL for the WebDAV upload
        string url = $"{serverUrl}/{path}/{filename}";

        // Create a new HttpRequestMessage with the PUT method and URL
        var request = new HttpRequestMessage(HttpMethod.Put, url);

        // Set the request content to the stream of the uploaded file
        request.Content = new StreamContent(formFile.OpenReadStream());

        // Send the HTTP request and await the response
        var response = await client.SendAsync(request);

        // Check if the response was successful
        if (response.IsSuccessStatusCode == false)
        {
            var content = await response.Content.ReadAsStringAsync();
            var error = ParseWebDavResponse(content);
            throw new Exception(error.ErrorMessage);
        }

        return new ()
        {
            FullPath = path + "/" + filename,
            Size = formFile.Length,
            Extension = Path.GetExtension(filename),
            MimeType = formFile.ContentType,
            Created = DateTime.UtcNow,
            Name = filename
        };
    }


    /// <summary>
    /// Deletes a file
    /// </summary>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>true if the files no longer exists afterwards</returns>
    public async Task<bool> Delete(string fullPath)
    {
        string requestUrl = serverUrl + "/" + fullPath;
        var request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
    
        try
        {
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to delete file {fullPath}. Status code: {response.StatusCode}");
            }

            return true;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to delete file {fullPath}. Error message: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a folder
    /// </summary>
    /// <param name="path">the full path of the file</param>
    public async Task CreateFolder(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        string url = serverUrl + "/" + path + "/";
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Method = new CustomHttpMethod("MKCOL");
        var response = await client.SendAsync(request);

        // Check if the response is successful
        if (response.IsSuccessStatusCode)
            return;

        string body = await response.Content.ReadAsStringAsync();
        var wdr = ParseWebDavResponse(body);
        if (wdr.Error)
            throw new Exception(wdr.ErrorMessage);
    }
    

    /// <summary>
    /// Tests a file storage connection
    /// </summary>
    /// <param name="provider">the provider for the file storage</param>
    /// <param name="url">the URL to the file storage server</param>
    /// <param name="username">the username for the file storage server</param>
    /// <param name="password">the password for the file storage server</param>
    /// <returns></returns>
    public static async Task<(bool Success, string Error)> Test(string provider, string url, string username, string password)
    {
        string serverUrl = GetServerUrl(provider, url, username, password);
        try
        {
            var client = GetClient(username, password);
            var propfindContent = new StringContent("<?xml version=\"1.0\" encoding=\"utf-8\" ?><D:propfind xmlns:D=\"DAV:\"><D:prop><D:displayname/></D:prop></D:propfind>", Encoding.UTF8, "text/xml");
            var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod("PROPFIND"), serverUrl) { Content = propfindContent });

            if (response.IsSuccessStatusCode)
                return (true, string.Empty);
            
            return (false, response.StatusCode.ToString());
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Cleans a full path
    /// </summary>
    /// <param name="path">the full path</param>
    /// <returns>the cleaned full path</returns>
    private string CleanFullPath(string path)
    {
        if (path == null)
            return string.Empty;
        if (provider.ToLowerInvariant() == "nextcloud")
        {
            string prefix = "/remote.php/dav/files/" + username + "/";
            if (path.StartsWith(prefix))
                return path[prefix.Length..];
        }
        return path;
    }

    /// <summary>
    /// Gets a thumbnail url
    /// </summary>
    /// <param name="path">the file path</param>
    /// <returns>the thumbnail URL</returns>
    private string GetThumnailUrl(string path)
    {
        if (path == null)
            return string.Empty;
        if (provider.ToLowerInvariant() == "nextcloud")
        {
            string url = this.baseUrl;
            if (url.EndsWith("/") == false)
                url += "/";
            url += "index.php/core/preview.png?file=" + HttpUtility.UrlEncode(path) + "&x=240&y=240&a=1&mode=cover";
            return url;
        }

        return serverUrl + "/" + path;
    }
    
    private static string GetServerUrl(string provider, string url, string username, string password)
    {
        if (string.IsNullOrEmpty(provider) || provider == "Custom")
            return url;

        if (provider.ToLowerInvariant() == "nextcloud")
        {
            if (url.Contains(".php"))
                return url;
            if (url.EndsWith("/") == false)
                url += "/";
            return url + "remote.php/dav/files/" + HttpUtility.UrlEncode(username);
        }
        
        return url;
    }
    /// <summary>
    /// Parses the given XML string into a WebDavResult object.
    /// </summary>
    /// <param name="xml">The XML string to parse.</param>
    /// <returns>A WebDavResult object representing the parsed XML.</returns>
    public static WebDavResult ParseWebDavResponse(string xml)
    {
        var result = new WebDavResult();

        XDocument doc = XDocument.Parse(xml);

        if (doc.Root.Name.LocalName == "error" &&
            doc.Root.Name.NamespaceName == "DAV:")
        {
            result.Error = true;
            result.ErrorMessage = doc.Root.Element("{http://sabredav.org/ns}message").Value;
        }

        return result;
    }

    
    
    /// <summary>
    /// A class that creates a custom HTTP method 
    /// </summary>
    public class CustomHttpMethod : HttpMethod
    {
        /// <summary>
        /// Creates a new custom HTTP method
        /// </summary>
        /// <param name="method">the method name</param>
        public CustomHttpMethod(string method)
            : base(method)
        {
        }
    }
    
    /// <summary>
    /// Represents the result of a WebDAV operation.
    /// </summary>
    public class WebDavResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation resulted in an error.
        /// </summary>
        public bool Error { get; set; }

        /// <summary>
        /// Gets or sets the error message if the operation resulted in an error.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}