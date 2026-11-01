
using Reflect.Domain.Repositories;
using Reflect.Domain.Services;

namespace Reflect.Web.Services;

public class PatientAuthorizationService : IPatientAuthorizationService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IPatientAccessRepository _patientAccessRepository;

    public PatientAuthorizationService(
        ICurrentUserService currentUserService,
        IPatientAccessRepository patientAccessRepository)
    {
        _currentUserService = currentUserService;
        _patientAccessRepository = patientAccessRepository;
    }

    public async Task<bool> CanAccessPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var clinicianId = _currentUserService.ClinicianId;

        if (!clinicianId.HasValue)
        {
            return false;
        }

        return await _patientAccessRepository.CanAccessPatientAsync(
            clinicianId.Value,
            patientId,
            cancellationToken);
    }
}
