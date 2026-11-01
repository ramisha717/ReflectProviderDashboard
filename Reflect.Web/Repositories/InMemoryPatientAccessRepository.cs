
using Reflect.Domain.Repositories;

namespace Reflect.Web.Repositories;

public class InMemoryPatientAccessRepository : IPatientAccessRepository
{
    private static readonly Guid DemoClinicianId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");

    private static readonly Guid DemoPatientId =
        Guid.Parse("10000000-0000-0000-0000-000000000001");

    private readonly Dictionary<Guid, HashSet<Guid>> _patientAccess = new()
    {
        [DemoClinicianId] = new HashSet<Guid>
        {
            DemoPatientId
        }
    };

    public Task<bool> CanAccessPatientAsync(
        Guid clinicianId,
        Guid patientId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        bool isAuthorized =
            _patientAccess.TryGetValue(clinicianId, out var patientIds)
            && patientIds.Contains(patientId);

        return Task.FromResult(isAuthorized);
    }
}
