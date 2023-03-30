using System.IO.Compression;

namespace Fenrus.Services;

/// <summary>
/// Service that updates apps from the github repository
/// </summary>
public class AppUpdaterService
{
    /// <summary>
    /// Update the apps
    /// </summary>
    /// <returns>the result as a string</returns>
    public async Task<(string Error, int Original, int Updated, int TotalApps)> Update()
    {
        int original = GetAppCount();
        var error = await DownloadAndExtract();
        if (string.IsNullOrEmpty(error) == false)
            return (error, original, 0, AppService.Apps.Count);
        int updated = GetAppCount();
        
        // reinitialize the apps
        AppService.Initialize();
        Logger.ILog(
            $"Updated {AppService.Apps.Count} applications (${updated - original} new app{(updated - original == 1 ? string.Empty : "s")})");
        return (string.Empty, original, updated, AppService.Apps.Count);
    }

    /// <summary>
    /// Gets the number of apps
    /// </summary>
    /// <returns>the number of apps</returns>
    private int GetAppCount()
        => Directory.GetDirectories(DirectoryHelper.GetAppsDirectory(), "*", SearchOption.AllDirectories).Length;

    /// <summary>
    /// Downloads the apps.zip and extracts it to the apps directory
    /// </summary>
    /// <returns>true if successful, otherwise false</returns>
    private async Task<string> DownloadAndExtract()
    {
        try
        {
            var url = "https://github.com/revenz/Fenrus/raw/master/apps.zip?t=" + DateTime.Now.ToOADate();
            using var client = new HttpClient();
            using var stream = new MemoryStream(await client.GetByteArrayAsync(url));
            string appsDir = DirectoryHelper.GetAppsDirectory();

            using var archive = new ZipArchive(stream);
            var appJsonEntry = archive.Entries.First(x => x.FullName == "apps.json");
            var appJson = GetStringContent(appJsonEntry);
            var appInfo = JsonSerializer.Deserialize<AppsInfo>(appJson);
            if (appInfo.MinimumVersion > new Version(Globals.Version))
                return "UpdateRequired";
            
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName == "apps.json")
                    continue; // dont extract this file
                var path = Path.Combine(appsDir, entry.FullName);
                if (entry.Length == 0)
                    Directory.CreateDirectory(path);
                else
                    entry.ExtractToFile(path, true);
            }
            Logger.ILog($"Extracted {archive.Entries.Count} entries");

            return string.Empty;
        }
        catch (Exception ex)
        {
            Logger.WLog("Failed extracting apps.zip from github: " + ex.Message);
            return "FailedToDownload";
        }
    }

    private string GetStringContent(ZipArchiveEntry entry)
    {
        using StreamReader reader = new StreamReader(entry.Open());
        string result = reader.ReadToEnd();
        return result;
    }

    /// <summary>
    /// Information about the apps that are downloaded
    /// </summary>
    class AppsInfo
    {
        /// <summary>
        /// Gets the minimum version supported for these plugins
        /// </summary>
        public Version MinimumVersion { get; init; }
    }
}