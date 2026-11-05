using Reflect.Domain.Services;

namespace Reflect.Web.Services;

// Demo identity mapping. The host can replace this implementation on integration.
public sealed class CurrentClinicianUserService : ICurrentClinicianUserService
{
    private readonly ICurrentUserService _currentUser;

    public CurrentClinicianUserService(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public Guid? ClinicianId => _currentUser.UserId ?? DemoClinicianId;

    private static readonly Guid DemoClinicianId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");
}
