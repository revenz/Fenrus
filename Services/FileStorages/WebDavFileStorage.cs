using System.Net;
using System.Net.Http.Headers;
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
    private string renameBase = string.Empty;
    private string searchUrl = string.Empty;
    private string searchPrefix = string.Empty;
    private string searchWildcard = "*";
    private HttpClient client;

    public WebDavFileStorage(string provider, string url, string username, string password)
    {
        this.baseUrl = url;
        this.provider = provider;
        this.username = username;
        this.password = password;
        InitializeProvider();
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
    /// Gets information for a single file or folder
    /// </summary>
    /// <param name="path">the file of the file or folder</param>
    /// <returns>the info</returns>
    public async Task<UserFile?> GetFile(string path)
    {
        // Build the WebDAV request message
        var request = new HttpRequestMessage(new HttpMethod("PROPFIND"), serverUrl + "/" + path);
        request.Content =
            new StringContent(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><d:propfind xmlns:d=\"DAV:\"><d:prop><d:getlastmodified/><d:getcontenttype/><d:getcontentlength/></d:prop></d:propfind>");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

        // Send the WebDAV request and await the response
        var response = await client.SendAsync(request);

        // Parse the WebDAV response XML and extract the file info
        if (response.IsSuccessStatusCode == false)
        {

        }
        var userFile = new UserFile()
        {
            FullPath = path,
            Name = new FileInfo(path).Name,
        };
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var doc = new XmlDocument();
        doc.LoadXml(responseContent);
        var ns = new XmlNamespaceManager(doc.NameTable);
        ns.AddNamespace("d", "DAV:");
        var lastModifiedNode = doc.SelectSingleNode("//d:getlastmodified", ns);
        var contentTypeNode = doc.SelectSingleNode("//d:getcontenttype", ns);
        var contentLengthNode = doc.SelectSingleNode("//d:getcontentlength", ns);
        
        if(DateTime.TryParse(lastModifiedNode?.InnerText ?? string.Empty, out DateTime createdUtc))
            userFile.Created = createdUtc;

        userFile.MimeType = contentTypeNode?.InnerText ?? string.Empty;
        if (long.TryParse(contentLengthNode?.InnerText ?? string.Empty, out var size))
            userFile.Size = size;

        var isFolder = path.EndsWith("/") || path.EndsWith("\\");
        if (isFolder)
            userFile.MimeType = "folder";
        else
        {
            userFile.Extension = new FileInfo(path)?.Extension?.TrimStart('.') ?? string.Empty;
        }

        return userFile;
    }

    /// <summary>
    /// Searches for files matching a search pattern
    /// </summary>
    /// <param name="path">the path to perform the search from</param>
    /// <param name="searchPattern">the searchPatten</param>
    /// <returns>a list of matching files</returns>
    public async Task<List<UserFile>> SearchFiles(string path, string searchPattern)
    {
        // Escape search pattern for XML
        searchPattern = searchPattern.Replace("*", this.searchWildcard);
        if (searchPattern.StartsWith(searchWildcard) == false && searchWildcard.EndsWith(searchWildcard))
            searchPattern = searchWildcard + searchPattern + searchWildcard;
        
        string escapedSearchPattern = WebUtility.HtmlEncode(searchPattern);
        HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("SEARCH"), searchUrl);
        request.Headers.Add("Depth", "1");
        request.Content = new StringContent(
            $@"<?xml version=""1.0"" encoding=""UTF-8"" ?>
            <d:searchrequest xmlns:d=""DAV:"">
            <d:basicsearch>
                <d:select>
                    <d:prop>
                        <d:getcontenttype />
                        <d:getcontentlength />
                        <d:creationdate />
                        <d:getlastmodified />
                    </d:prop>
                </d:select>
                <d:from>
                    <d:scope>
                        <d:href>{(searchPrefix + path).TrimEnd('/')}</d:href>
                        <d:depth>infinity</d:depth>
                    </d:scope>
                </d:from>
                <d:where>
                    <d:like>
                        <d:prop>
                            <d:displayname/>
                        </d:prop>
                        <d:literal>{escapedSearchPattern}</d:literal>
                    </d:like>
                </d:where>
            </d:basicsearch>
        </d:searchrequest>", Encoding.UTF8, "application/xml");
    
        // Execute the request and parse the response
        HttpResponseMessage response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode == false)
        {
            throw new Exception(content);
        }

        var xml = XDocument.Parse(content);
        
        XNamespace ns = "DAV:";

        List<UserFile> files = new();
        foreach (var node in xml.Descendants(ns + "response"))
        {
            UserFile userFile = new ();
            userFile.FullPath = CleanFullPath((string)node.Descendants(ns + "href").FirstOrDefault()?.Value);
            if (string.IsNullOrEmpty(userFile.FullPath))
                continue;
            if (userFile.FullPath.EndsWith("/"))
                continue; // not interested in folders
            userFile.Name = Path.GetFileName(userFile.FullPath);
            userFile.Extension = Path.GetExtension(userFile.FullPath).TrimStart('.') ;

            if (DateTime.TryParse(node.Descendants(ns + "creationdate").FirstOrDefault()?.Value  ?? string.Empty, out DateTime dt)) 
                userFile.Created = dt;
            
            userFile.MimeType  = node.Descendants(ns + "getcontenttype")?.FirstOrDefault()?.Value ?? string.Empty;
            if (long.TryParse(node.Descendants(ns + "getcontentlength").FirstOrDefault()?.Value ?? string.Empty,
                    out long size))
                userFile.Size = size;
            files.Add(userFile);
        }

        foreach (UserFile file in files)
        {
        }

        return files;
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
            url += "/" + Uri.EscapeDataString(folder);
        }

        var propfindContent = new StringContent(
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?><d:propfind xmlns:d=\"DAV:\"><d:prop><d:displayname/><d:getlastmodified/><d:getcontenttype/><d:getcontentlength/></d:prop></d:propfind>",
            Encoding.UTF8, "text/xml");
        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod("PROPFIND"), url)
            { Content = propfindContent });

        var xmlResponse = await response.Content.ReadAsStringAsync();
        var doc = new XmlDocument();
        doc.LoadXml(xmlResponse);
        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("d", "DAV:");
        var nodes = doc.DocumentElement.SelectNodes("//d:response", nsmgr);

        var files = new List<UserFile>();
        foreach (XmlNode node in nodes)
        {
            var displayNameNode = node.SelectSingleNode("d:propstat/d:prop/d:displayname", nsmgr);
            var contentTypeNode = node.SelectSingleNode("d:propstat/d:prop/d:getcontenttype", nsmgr);
            var contentLengthNode = node.SelectSingleNode("d:propstat/d:prop/d:getcontentlength", nsmgr);
            var lastModifiedNode = node.SelectSingleNode("d:propstat/d:prop/d:getlastmodified", nsmgr);

            if (displayNameNode == null || string.IsNullOrEmpty(displayNameNode.InnerText))
                continue;
            
            var fullPath = CleanFullPath(node.SelectSingleNode("d:href", nsmgr)?.InnerText);
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

        string url = serverUrl + "/" + Uri.EscapeDataString(fullPath) + "?x=thumbnail";
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
        path = path.TrimEnd('/');

        if (path != string.Empty)
        {
            if (await FolderExists(path) == false)
            {
                // need to create the folder
                await CreateFolder(path);
            }
        }
        string url = $"{serverUrl}/{Uri.EscapeDataString(path)}/{Uri.EscapeDataString(filename)}";

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
        string requestUrl = serverUrl + "/" + Uri.EscapeDataString(fullPath);
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
    /// Renames a file or folder
    /// </summary>
    /// <param name="path">the full path to the file or folder</param>
    /// <param name="dest">the new full path for the file or folder</param>
    /// <returns>an awaited task</returns>
    public async Task Rename(string path, string dest)
    {
        // Set up the request message with the appropriate method and headers
        var request = new HttpRequestMessage(new CustomHttpMethod("MOVE"), serverUrl + "/" + path);
        request.Headers.Add("Destination", this.renameBase + dest);
        request.Headers.Add("Overwrite", "F"); // prevents overwriting an existing item with the same name

        // Send the request using the preconfigured HttpClient
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
            return; 
        
        string body = await response.Content.ReadAsStringAsync();
        var wdr = ParseWebDavResponse(body);
        throw new Exception(wdr.ErrorMessage?.EmptyAsNull() ?? "Failed to rename");
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
            var propfindContent = new StringContent("<?xml version=\"1.0\" encoding=\"utf-8\" ?><d:propfind xmlns:d=\"DAV:\"><d:prop><d:displayname/></d:prop></d:propfind>", Encoding.UTF8, "text/xml");
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
                path = path[prefix.Length..];
        }

        path = path.Replace("%20", " ");
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


    private void InitializeProvider()
    {
        this.serverUrl = GetServerUrl(provider, baseUrl, username, password);
        if (this.provider?.ToLowerInvariant() == "nextcloud")
        {
            this.renameBase = "/remote.php/dav/files/" + username + "/";
            this.searchPrefix = "/files/" + username + "/";
            this.searchUrl = this.baseUrl + "/remote.php/dav/";
            this.searchWildcard = "%";
        }
        else
        {
            this.renameBase = string.Empty;
            this.searchUrl = this.serverUrl;
            this.searchPrefix = string.Empty;
            this.searchWildcard = "*";
        }
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
    /// Check if a folder exists in WebDAV at the specified path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the folder exists, false otherwise.</returns>
    public async Task<bool> FolderExists(string path)
    {
        string url = serverUrl + "/" + path + "/";

        // Build the PROPFIND request XML.
        string requestXml = $@"<?xml version=""1.0""?>
        <d:propfind xmlns:d=""DAV:"">
            <d:prop>
                <d:resourcetype/>
            </d:prop>
        </d:propfind>";

        // Create a new HttpRequestMessage with the required headers and request XML.
        var request = new HttpRequestMessage(new CustomHttpMethod("PROPFIND"), url);
        request.Headers.Add("Depth", "0");
        request.Content = new StringContent(requestXml, Encoding.UTF8, "text/xml");

        // Send the request using the configured HttpClient and await the response.
        var response = await client.SendAsync(request);

        // Check if the response indicates the folder exists (a 207 Multi-Status response with a d:collection element).
        if (response.StatusCode == HttpStatusCode.MultiStatus)
        {
            string responseXml = await response.Content.ReadAsStringAsync();

            XDocument doc = XDocument.Parse(responseXml);
            XNamespace ns = "DAV:";

            XElement collectionElement = doc.Descendants(ns + "collection").FirstOrDefault();

            return collectionElement != null;
        }

        return false;
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