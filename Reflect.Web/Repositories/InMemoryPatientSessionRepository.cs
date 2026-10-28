
using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;

namespace Reflect.Web.Repositories;

public class InMemoryPatientSessionRepository : IPatientSessionRepository
{
    private readonly List<PatientSession> _sessions = new();

    public InMemoryPatientSessionRepository()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        Guid patient1Id =
            Guid.Parse("10000000-0000-0000-0000-000000000001");

        Guid patient2Id =
            Guid.Parse("10000000-0000-0000-0000-000000000002");

        Guid patient3Id =
            Guid.Parse("10000000-0000-0000-0000-000000000003");

        Guid patient4Id =
            Guid.Parse("10000000-0000-0000-0000-000000000004");

        // Patient 1: Higher activity in the preceding 30-day period.
        for (int i = 0; i < 12; i++)
        {
            _sessions.Add(new PatientSession
            {
                PatientId = patient1Id,
                StartedAt = now.AddDays(-59 + i * 2),
                Platform = i % 2 == 0 ? "Reflect XR" : "Reflect Mobile",
                ModuleName = "Guided Relaxation",
                DurationMinutes = 20,
                RecordingStatus = "Succeeded"
            });
        }

        // Patient 1: Lower activity in the most recent 30-day period.
        for (int i = 0; i < 4; i++)
        {
            _sessions.Add(new PatientSession
            {
                PatientId = patient1Id,
                StartedAt = now.AddDays(-26 + i * 7),
                Platform = i % 2 == 0 ? "Reflect XR" : "Reflect Mobile",
                ModuleName = "Breathing Exercise",
                DurationMinutes = 15,
                RecordingStatus = "Succeeded"
            });
        }

        // Patient 2: Regular ongoing engagement.
        for (int i = 0; i < 16; i++)
        {
            _sessions.Add(new PatientSession
            {
                PatientId = patient2Id,
                StartedAt = now.AddDays(-56 + i * 3),
                Platform = i % 2 == 0 ? "Reflect XR" : "Reflect Mobile",
                ModuleName = "Guided Relaxation",
                DurationMinutes = 25,
                RecordingStatus = "Succeeded"
            });
        }

        // Patient 3: Historical sessions for a former patient.
        for (int i = 0; i < 6; i++)
        {
            _sessions.Add(new PatientSession
            {
                PatientId = patient3Id,
                StartedAt = now.AddDays(-80 + i * 5),
                Platform = "Reflect Mobile",
                ModuleName = "Breathing Exercise",
                DurationMinutes = 18,
                RecordingStatus = "Succeeded"
            });
        }

        // Patient 4: Previously engaged, but no recent activity.
        for (int i = 0; i < 5; i++)
        {
            _sessions.Add(new PatientSession
            {
                PatientId = patient4Id,
                StartedAt = now.AddDays(-48 + i * 6),
                Platform = "Reflect XR",
                ModuleName = "Guided Relaxation",
                DurationMinutes = 20,
                RecordingStatus = "Succeeded"
            });
        }
    }

    public Task<IReadOnlyList<PatientSession>> GetSessionsByPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<PatientSession> results = _sessions
            .Where(session => !session.IsDeleted)
            .Where(session => session.PatientId == patientId)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<IReadOnlyList<PatientSession>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<PatientSession> results = _sessions
            .Where(session => !session.IsDeleted)
            .ToList();

        return Task.FromResult(results);
    }
}
