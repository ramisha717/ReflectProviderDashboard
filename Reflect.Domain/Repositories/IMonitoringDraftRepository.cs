
using Reflect.Domain.Entities;

namespace Reflect.Domain.Repositories;

public interface IMonitoringDraftRepository
{
    Task<MonitoringDraft> GetByPatientAndClinicianAsync(
      Guid patientId,
      Guid clinicianId,
      CancellationToken cancellationToken);

    Task AddAsync(
      MonitoringDraft draft,
      CancellationToken cancellationToken);

    Task UpdateAsync(
      MonitoringDraft draft,
      CancellationToken cancellationToken);

    Task DeleteAsync(
      Guid draftId,
      CancellationToken cancellationToken);
}
