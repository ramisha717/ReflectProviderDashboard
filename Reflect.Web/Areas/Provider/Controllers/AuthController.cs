
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
[Route("Provider/[controller]")]
public class AuthController : Controller
{
    private static readonly Guid DemoClinicianId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<IActionResult> Login()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, DemoClinicianId.ToString()),
            new(ClaimTypes.Role, "Clinician")
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return Ok(new { message = "Demo clinician signed in." });
    }

    [Authorize]
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return Ok(new { message = "Signed out." });
    }
}
