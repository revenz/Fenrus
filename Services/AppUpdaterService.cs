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
    public async Task<(bool Success, int Original, int Updated, int TotalApps)> Update()
    {
        int original = GetAppCount();
        bool success = await DownloadAndExtract();
        if (success == false)
            return (false, original, 0, AppService.Apps.Count);
        int updated = GetAppCount();
        
        // reinitialize the apps
        AppService.Initialize();
        Logger.ILog(
            $"Updated {AppService.Apps.Count} applications (${updated - original} new app{(updated - original == 1 ? string.Empty : "s")})");
        return (true, original, updated, AppService.Apps.Count);
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
    private async Task<bool> DownloadAndExtract()
    {
        try
        {
            var url = "https://github.com/revenz/Fenrus/raw/master/apps.zip?t=" + DateTime.Now.ToOADate();
            using var client = new HttpClient();
            using var stream = new MemoryStream(await client.GetByteArrayAsync(url));
            string appsDir = DirectoryHelper.GetAppsDirectory();

            using var archive = new ZipArchive(stream);
            foreach (var entry in archive.Entries)
            {
                var path = Path.Combine(appsDir, entry.FullName);
                if (entry.Length == 0)
                    Directory.CreateDirectory(path);
                else
                    entry.ExtractToFile(path, true);
            }
            Logger.ILog($"Extracted {archive.Entries.Count} entries");

            return true;
        }
        catch (Exception ex)
        {
            Logger.WLog("Failed extracting apps.zip from github: " + ex.Message);
            return false;
        }
    }
}