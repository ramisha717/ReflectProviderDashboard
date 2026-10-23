using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;

namespace Reflect.Web.Repositories;

public class InMemoryPatientSessionRepository : IPatientSessionRepository
{
  private readonly List<PatientSession> _sessions = new()
 {
    new PatientSession
    {
        PatientId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
        StartedAt = new DateTimeOffset(2026, 10, 20, 14, 30, 0, TimeSpan.Zero),
        Platform = "Reflect XR",
        ModuleName = "Guided Relaxation",
        DurationMinutes = 20,
        RecordingStatus = "Succeeded"
    },

    new PatientSession
    {
        PatientId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
        StartedAt = new DateTimeOffset(2026, 10, 21, 16, 0, 0, TimeSpan.Zero),
        Platform = "Reflect Mobile",
        ModuleName = "Breathing Exercise",
        DurationMinutes = 15,
        RecordingStatus = "Succeeded"
    },

    new PatientSession
    {
        PatientId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
        StartedAt = new DateTimeOffset(2026, 10, 19, 11, 0, 0, TimeSpan.Zero),
        Platform = "Reflect XR",
        ModuleName = "Guided Relaxation",
        DurationMinutes = 25,
        RecordingStatus = "Succeeded"
    },

    new PatientSession
    {
        PatientId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
        StartedAt = new DateTimeOffset(2026, 10, 21, 10, 30, 0, TimeSpan.Zero),
        Platform = "Reflect XR",
        ModuleName = "Breathing Exercise",
        DurationMinutes = 10,
        RecordingStatus = "Failed"
    },

    new PatientSession
    {
        PatientId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
        StartedAt = new DateTimeOffset(2026, 10, 22, 13, 0, 0, TimeSpan.Zero),
        Platform = "Reflect Mobile",
        ModuleName = "Guided Relaxation",
        DurationMinutes = 18,
        RecordingStatus = "Succeeded"
    }
 };

  public Task<IReadOnlyList<PatientSession>> GetSessionsByPatientAsync(
    Guid patientId,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    IReadOnlyList<PatientSession> results = _sessions.Where(session => !session.IsDeleted).Where(session => session.PatientId == patientId).ToList();

    return Task.FromResult(results);
  }
}