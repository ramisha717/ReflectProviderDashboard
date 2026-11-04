
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Reflect.Web.Standins;

public static class DemoClinicianAuthenticationExtensions
{
    public static IApplicationBuilder UseDemoClinicianAuthentication(
        this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                var claims = new List<Claim>
                {
                    new(
                        ClaimTypes.NameIdentifier,
                        "20000000-0000-0000-0000-000000000001"),
                    new(ClaimTypes.Role, "Clinician"),
                    new(ClaimTypes.Name, "Dr. Sarah Mitchell")
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                context.User = new ClaimsPrincipal(identity);
            }

            await next();
        });
    }
}

