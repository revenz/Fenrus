using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Theme Controller
/// </summary>
[Route("themes")]
public class ThemeController:Controller
{
    [HttpGet("{name}/theme.css")]
    [ResponseCache(Duration = 31 * 24 * 60 * 60)]
    public IActionResult Css([FromRoute]string name)
    {
        var theme = new Services.ThemeService().GetTheme(name);
        if (theme?.Css?.Any() != true)
            return Content("", "text/css");

        string css = string.Empty;
        foreach (var file in theme.Css)
        {
            string ff = Path.Combine(theme.Directory, file);
            if (System.IO.File.Exists(ff) == false)
                continue;
            css += "/* " + file + "*/\n" + System.IO.File.ReadAllText(ff) + "\n";
        }

        return Content(css, "text/css");
    }
}