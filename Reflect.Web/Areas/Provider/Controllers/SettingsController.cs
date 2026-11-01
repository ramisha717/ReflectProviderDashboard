using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
[Authorize(Policy = "ClinicianOnly")]
public class SettingsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View("Index");
    }

    [HttpGet]
    public IActionResult IndexPage()
    {
        return View("Index");
    }
}
