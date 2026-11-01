
using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;

namespace Reflect.Web.Repositories;

public class InMemoryMonitoringDraftRepository : IMonitoringDraftRepository
{
  private readonly List<MonitoringDraft> _drafts = new();

  public Task<MonitoringDraft?> GetByPatientAndClinicianAsync(
      Guid patientId,
      Guid clinicianId,
      CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    MonitoringDraft? draft = _drafts.FirstOrDefault(
        d => !d.IsDeleted
            && d.PatientId == patientId
            && d.ClinicianId == clinicianId);

    return Task.FromResult(draft);
  }

  public Task AddAsync(
      MonitoringDraft draft,
      CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    if (_drafts.Any(d => !d.IsDeleted
        && d.PatientId == draft.PatientId
        && d.ClinicianId == draft.ClinicianId))
    {
        throw new InvalidOperationException(
            "An active draft already exists for this patient and clinician.");
    }

    _drafts.Add(draft);

    return Task.CompletedTask;
  }

  public Task UpdateAsync(
      MonitoringDraft draft,
      CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    MonitoringDraft? existing = _drafts.FirstOrDefault(
        d => d.Id == draft.Id && !d.IsDeleted);

    if (existing == null)
    {
        throw new EntityNotFoundException(
            "Monitoring draft not found.");
    }

    existing.Notes = draft.Notes;
    existing.LastModifiedAt = DateTimeOffset.UtcNow;

    return Task.CompletedTask;
  }

  public Task DeleteAsync(
      Guid draftId,
      CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    MonitoringDraft? existing = _drafts.FirstOrDefault(
      d => d.Id == draftId && !d.IsDeleted);

    if (existing == null)
    {
      throw new EntityNotFoundException(
        "Monitoring draft not found.");
    }

    existing.IsDeleted = true;
    existing.DateDeleted = DateTimeOffset.UtcNow;

    return Task.CompletedTask;
  }
}
