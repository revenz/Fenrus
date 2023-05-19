using Fenrus.Models;
using Fenrus.Terminals;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for Terminals (docker or SSH)
/// </summary>
[Route("terminal")]
[Authorize]
public class TerminalController : BaseController
{
    /// <summary>
    /// Opens a terminal connection
    /// </summary>
    /// <param name="uid">the Uid of the app container</param>
    /// <param name="rows">the number of rows for the terminal</param>
    /// <param name="cols">the number of columns for the terminal</param>
    [HttpGet("{uid}")]
    public async Task Get([FromRoute] Guid uid, [FromQuery] int rows = 24, [FromQuery] int cols = 24)
    {
        var settings = GetUserSettings();
        if (settings == null)
            throw new UnauthorizedAccessException();

        var groups = new GroupService().GetAllForUser(settings.UserUid);
        
        var item = groups?.SelectMany(x => x.Items)?.FirstOrDefault(x => x.Uid == uid);
        if (item == null)
        {
            // try and find the item from the system groups
            item = new GroupService().GetSystemGroups(enabledOnly: true)
                .SelectMany(x => x.Items).FirstOrDefault(x => x.Uid == uid);
        }

        if (item == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        Terminal? terminal = null;
        using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
        if (item is AppItem app)
        {
            if (string.IsNullOrEmpty(app.DockerContainer) == false && app.DockerUid != null)
            {
                var docker = new DockerService().GetByUid(app.DockerUid.Value);
                if (docker == null)
                {
                    var translator = GetTranslator(settings);
                    throw new Exception(translator.Instant("ErrorMessages.DockerServerNotFound",
                        new { uid = app }));
                }

                terminal = new DockerTerminal(ws, rows, cols, docker.Address, docker.Port, app.DockerContainer, app.DockerCommand);
            }
            else if (string.IsNullOrEmpty(app.SshServer) == false)
            {
                terminal = new SshTerminal(ws, rows, cols, app.SshServer, 0, app.SshUserName, 
                    EncryptionHelper.Decrypt(app.SshPassword));
            }
        }
        
        if(terminal == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await terminal.Connect();
    }
    
    
    /// <summary>
    /// Opens a terminal connection
    /// </summary>
    /// <param name="rows">the number of rows for the terminal</param>
    /// <param name="cols">the number of columns for the terminal</param>
    [HttpGet("ssh")]
    public async Task Ssh([FromQuery] string info, [FromQuery] int rows = 24, [FromQuery] int cols = 24)
    {
        var profile = GetUserProfile();
        if (profile == null)
            throw new UnauthorizedAccessException();

        string decrypted = EncryptionHelper.DecryptAes(info);
        var serverInfo = JsonSerializer.Deserialize<SshInfo>(decrypted, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

        if (Guid.TryParse(serverInfo.User, out Guid uid))
        {
            var app = profile.AppGroups.SelectMany(x => x.Items).FirstOrDefault(x => x.Uid == uid);
            if (app != null)
            {
                (serverInfo.User, serverInfo.Password) = ParseAddress(app.Address, serverInfo.User, serverInfo.Password);
            }
        }
            
        

        using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var terminal = new SshTerminal(ws, rows, cols, serverInfo.Server, 0, serverInfo.User, serverInfo.Password);
        await terminal.Connect();
    }

    /// <summary>
    /// Parses an address and gets the username/password from it if set
    /// </summary>
    /// <param name="address">the address</param>
    /// <param name="currentUser">the current username to return if not set here</param>
    /// <param name="currentPassword">the current password to return if not set here</param>
    /// <returns>the username and password if available</returns>
    private (string User, string Password) ParseAddress(string address, string currentUser, string currentPassword)
    {
        if (string.IsNullOrEmpty(address))
            return (currentUser, currentPassword);
        int atIndex = address.IndexOf("@");
        if (atIndex < 0)
            return (currentUser, currentPassword);
        string user = address[..atIndex];
        int caretIndex = user.IndexOf(":");
        if (caretIndex < 0)
            return (user, currentPassword);
        string pwd = user.Substring(caretIndex + 1);
        user = user[..caretIndex];
        return (user, pwd);
    }

    /// <summary>
    /// Opens a terminal log
    /// </summary>
    /// <param name="uid">the Uid of the app container</param>
    /// <param name="rows">the number of rows for the terminal</param>
    /// <param name="cols">the number of columns for the terminal</param>
    [HttpGet("log/{uid}")]
    public async Task GetLog([FromRoute] Guid uid, [FromQuery] int rows = 24, [FromQuery] int cols = 24)
    {
        var settings = GetUserSettings();
        if (settings == null)
            throw new UnauthorizedAccessException();

        var groups = new GroupService().GetAllForUser(settings.UserUid);
        
        var item = groups?.SelectMany(x => x.Items)?.FirstOrDefault(x => x.Uid == uid);
        if (item == null)
        {
            // try and find the item from the system groups
            item = new GroupService().GetSystemGroups(enabledOnly: true)
                .SelectMany(x => x.Items).FirstOrDefault(x => x.Uid == uid);
        }

        if (item == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        DockerTerminal? terminal = null;
        using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
        if (item is AppItem app)
        {
            if (string.IsNullOrEmpty(app.DockerContainer) == false && app.DockerUid != null)
            {
                var docker = new DockerService().GetByUid(app.DockerUid.Value);
                if (docker == null)
                {
                    var translator = GetTranslator(settings);
                    throw new Exception(translator.Instant("ErrorMessages.DockerServerNotFound",
                        new { uid = app }));
                }

                terminal = new DockerTerminal(ws, rows, cols, docker.Address, docker.Port, app.DockerContainer, app.DockerCommand);
            }
        }
        
        if(terminal == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await terminal.Log();
    }

    private class SshInfo
    {
        public string Server { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}