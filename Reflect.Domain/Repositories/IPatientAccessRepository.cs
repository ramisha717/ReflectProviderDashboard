
namespace Reflect.Domain.Repositories;

public interface IPatientAccessRepository
{
    Task<bool> CanAccessPatientAsync(
        Guid clinicianId,
        Guid patientId,
        CancellationToken cancellationToken);

    Task GrantAccessAsync(
    Guid clinicianId,
    Guid patientId,
    CancellationToken cancellationToken);
}
