using System.Security.Claims;
using Reflect.Domain.Services;

namespace Reflect.Web.Services;

public class CurrentUserService : ICurrentUserService
{
    private static readonly Guid DemoClinicianId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User =>
        _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var id = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var userId) ? userId : null;
        }
    }

    public string EntraObjectId =>
        User?.FindFirstValue("oid") ?? string.Empty;

    public string Email =>
        User?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public bool IsInRole(string role) =>
        User?.IsInRole(role) == true;

    public Guid? ClinicianId => UserId ?? DemoClinicianId;
}
