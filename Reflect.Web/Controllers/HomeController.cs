using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Reflect.Web.Models;

namespace Reflect.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("IndexPage", "Overview", new { area = "Provider" });
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
