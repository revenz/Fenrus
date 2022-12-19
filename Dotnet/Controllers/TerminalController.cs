using Fenrus.Models;
using Fenrus.Terminals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ClearScript.Util.Web;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for Terminals (docker or SSH)
/// </summary>
[Route("terminal")]
public class TerminalController :Controller
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
        // TODO: need to authorize this request
        var settings = DemoHelper.GetDemoUserSettings();
        var item = settings.Groups.SelectMany(x => x.Items)?.FirstOrDefault(x => x.Uid == uid);
        if (item == null)
        {
            // TODO: look for system item
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
            if (string.IsNullOrEmpty(app.DockerContainer) == false)
            {
                var docker = settings.Docker.FirstOrDefault(x => x.Uid == app.DockerUid);
                if (docker == null)
                    throw new Exception($"Docker app server not found: '{app.DockerUid}'");
                terminal = new DockerTerminal(ws, rows, cols, docker.Address, docker.Port, app.DockerContainer);
            }
            else if (string.IsNullOrEmpty(app.SshServer) == false)
            {
                terminal = new SshTerminal(ws, rows, cols, app.SshServer, 0, app.SshUserName, app.SshPassword);
            }
        }
        
        if(terminal == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await terminal.Connect();
    }
}