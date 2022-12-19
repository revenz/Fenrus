using Fenrus.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

[Route("/")]
public class HomeController:Controller
{
    [HttpGet]
    public IActionResult Home()
    {
#if(DEBUG)
        var settings = DemoHelper.GetDemoUserSettings();
        DashboardPageModel model = new DashboardPageModel
        {
            Dashboard = settings.Dashboards.First(),
            Settings = settings
        };
        return View("Dashboard", model);
#else
        Dashboard model = new Dashboard()
        {
            Name = "Test Dashboard"
        };
        return View("Dashboard", model);
#endif
    } 
}