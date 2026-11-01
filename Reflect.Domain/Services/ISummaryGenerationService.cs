
using Reflect.Domain.Entities;

namespace Reflect.Domain.Services;

public interface ISummaryGenerationService
{
    string GenerateSummary(
      IReadOnlyList<PatientSession> sessions,
      bool includeSessionCountAndDuration,
      bool includePlatformActivity,
      bool includeModuleActivity,
      bool includeClinicianObservations,
      IReadOnlyList<ClinicalObservation> clinicalObservations);
}
