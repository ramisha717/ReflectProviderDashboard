
namespace Reflect.Domain.Services;

public interface IPatientAuthorizationService
{
    Task<bool> CanAccessPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken);
}
