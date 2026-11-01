using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;

namespace Reflect.Web.Repositories;

public class InMemoryClinicalObservationRepository : IClinicalObservationRepository
{
    private readonly List<ClinicalObservation> _observations = new();

    public InMemoryClinicalObservationRepository()
    {
        Guid patient1Id =
            Guid.Parse("10000000-0000-0000-0000-000000000001");

        Guid patient2Id =
            Guid.Parse("10000000-0000-0000-0000-000000000002");

        Guid patient4Id =
            Guid.Parse("10000000-0000-0000-0000-000000000004");

        Guid clinicianId =
            Guid.Parse("20000000-0000-0000-0000-000000000001");

        _observations.Add(new ClinicalObservation
        {
            PatientId = patient1Id,
            ClinicianId = clinicianId,
            ObservedAt = DateTimeOffset.UtcNow.AddDays(-3),
            ObservationType = "Session Review",
            Notes = "Clinician reviewed recent engagement history. No clinical outcome assessment recorded."
        });

        _observations.Add(new ClinicalObservation
        {
            PatientId = patient2Id,
            ClinicianId = clinicianId,
            ObservedAt = DateTimeOffset.UtcNow.AddDays(-5),
            ObservationType = "Follow-up",
            Notes = "Clinician documented a planned follow-up to discuss the patient's experience with Reflect."
        });

        _observations.Add(new ClinicalObservation
        {
            PatientId = patient4Id,
            ClinicianId = clinicianId,
            ObservedAt = DateTimeOffset.UtcNow.AddDays(-8),
            ObservationType = "Clinical Observation",
            Notes = "Clinician recorded a discussion of the patient's self-reported experience. Further assessment may be needed."
        });
    }

    public Task<List<ClinicalObservation>> GetAllAsync(
        CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      return Task.FromResult(
        _observations
          .Where(o => !o.IsDeleted)
          .ToList());
    }

    public Task<List<ClinicalObservation>> GetByPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      return Task.FromResult(
        _observations
          .Where(o => !o.IsDeleted && o.PatientId == patientId)
          .ToList());
    }

    public Task AddAsync(
    ClinicalObservation observation,
    CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();

      _observations.Add(observation);

      return Task.CompletedTask;
    }
}