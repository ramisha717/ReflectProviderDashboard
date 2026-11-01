using System.Security.Claims;
using Reflect.Domain.Services;

namespace Reflect.Web.Services;

public class CurrentUserService : ICurrentUserService
{
    private static readonly Guid DemoClinicianId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? ClinicianId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(id, out var clinicianId))
                {
                    return clinicianId;
                }
            }

            return DemoClinicianId;
        }
    }
}
