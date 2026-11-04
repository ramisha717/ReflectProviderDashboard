
using Reflect.Domain.Repositories;

namespace Reflect.Web.Repositories;

public class InMemoryPatientAccessRepository : IPatientAccessRepository
{
    private static readonly Guid DemoClinicianId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");

    

    private readonly Dictionary<Guid, HashSet<Guid>> _patientAccess = new()
    {
        [DemoClinicianId] = new HashSet<Guid>
        {
            Guid.Parse("10000000-0000-0000-0000-000000000001"),
            Guid.Parse("10000000-0000-0000-0000-000000000002"),
            Guid.Parse("10000000-0000-0000-0000-000000000003")
        }
    };

    public Task<bool> CanAccessPatientAsync(
        Guid clinicianId,
        Guid patientId,
        CancellationToken cancellationToken)
    {
    cancellationToken.ThrowIfCancellationRequested();

    bool isAuthorized;

    lock (_patientAccess)
    {
        isAuthorized =
            _patientAccess.TryGetValue(clinicianId, out var patientIds)
            && patientIds.Contains(patientId);
    }

    return Task.FromResult(isAuthorized);
    }


    public Task GrantAccessAsync(
    Guid clinicianId,
    Guid patientId,
    CancellationToken cancellationToken)
    {
    cancellationToken.ThrowIfCancellationRequested();

    lock (_patientAccess)
    {
        if (!_patientAccess.TryGetValue(clinicianId, out var patientIds))
        {
            patientIds = new HashSet<Guid>();
            _patientAccess[clinicianId] = patientIds;
        }

        patientIds.Add(patientId);
    }

    return Task.CompletedTask;
    }
}
