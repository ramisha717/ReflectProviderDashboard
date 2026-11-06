using Reflect.Domain;
using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;

namespace Reflect.Web.Repositories;

public class InMemoryPatientSessionRepository : IPatientSessionRepository
{
    private readonly List<PatientSession> _sessions = new();

    public InMemoryPatientSessionRepository()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset baseUtc = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);

        Guid patient1Id = Guid.Parse("10000000-0000-0000-0000-000000000001");
        Guid patient2Id = Guid.Parse("10000000-0000-0000-0000-000000000002");
        Guid patient3Id = Guid.Parse("10000000-0000-0000-0000-000000000003");
        Guid patient4Id = Guid.Parse("10000000-0000-0000-0000-000000000004");

        // Seed comprehensive sessions covering all 17 XR modules and 14 Mobile modules
        // across varied hours of day (0-23) and days over the last 56 days.

        // Patient 1 Sessions (mix of XR and Mobile)
        var p1XrSchedule = new (int daysAgo, int hour, int minute, string module, int duration)[]
        {
            (1, 9, 15, "Containment", 22),
            (2, 14, 30, "Rhythmic Relaxation", 25),
            (3, 10, 0, "Color Psychology", 20),
            (4, 16, 45, "Flow", 30),
            (6, 11, 20, "Emotional Expression", 24),
            (7, 15, 10, "Self Compassion", 20),
            (8, 18, 0, "Catharsis", 28),
            (10, 8, 30, "Portal to the Subconscious", 25),
            (11, 13, 15, "Present Self Outside", 22),
            (13, 17, 40, "Present Self Inside", 26),
            (14, 9, 50, "Personal Strengths", 18),
            (16, 20, 10, "Future Self", 30),
            (17, 10, 30, "Appreciation of Life", 25),
            (19, 14, 0, "Hope Tree", 22),
            (21, 11, 45, "New Pathways", 27),
            (22, 16, 15, "Finding Balance", 24),
            (24, 8, 15, "Training Tutorial", 15),
            (26, 12, 30, "Containment", 20),
            (28, 19, 0, "Emotional Expression", 25),
            (31, 9, 0, "Flow", 28),
            (33, 15, 30, "Self Compassion", 22),
            (36, 10, 15, "Personal Strengths", 20),
            (38, 17, 0, "Hope Tree", 25),
            (41, 13, 45, "Finding Balance", 24),
            (44, 18, 30, "Rhythmic Relaxation", 22),
            (47, 11, 0, "Color Psychology", 26),
            (50, 16, 20, "Catharsis", 30),
            (53, 9, 45, "Future Self", 25)
        };

        foreach (var s in p1XrSchedule)
        {
            _sessions.Add(new PatientSession
            {
                PatientId = patient1Id,
                StartedAt = baseUtc.AddDays(-s.daysAgo).AddHours(s.hour).AddMinutes(s.minute),
                Platform = ReflectProductCatalog.PlatformXR,
                ModuleName = s.module,
                DurationMinutes = s.duration,
                RecordingStatus = "Succeeded"
            });
        }

        var p1MobileSchedule = new (int daysAgo, int hour, int minute, string module, int duration)[]
        {
            (1, 8, 0, "Floating Bubbles", 15),
            (2, 12, 15, "Finger Painting", 18),
            (4, 9, 30, "Pour Paint", 16),
            (5, 14, 0, "Breathing Mandala", 12),
            (7, 21, 15, "Photo Coloring (AI)", 20),
            (8, 11, 45, "AI Created Mandala (AI)", 22),
            (9, 16, 30, "Create Your Own Prompt (AI)", 25),
            (11, 19, 0, "Options Prompt Pathway (AI)", 18),
            (12, 8, 45, "Select from Images Pathway (AI)", 20),
            (14, 13, 0, "Creativity (AI)", 24),
            (15, 17, 30, "Emotions (AI)", 22),
            (17, 10, 15, "Self (AI)", 20),
            (18, 15, 0, "Journey (AI)", 25),
            (20, 21, 0, "Reflection Journals (AI)", 18),
            (22, 9, 15, "Floating Bubbles", 15),
            (23, 14, 45, "Photo Coloring (AI)", 20),
            (25, 18, 10, "Reflection Journals (AI)", 22),
            (27, 11, 30, "Create Your Own Prompt (AI)", 26),
            (30, 8, 30, "Breathing Mandala", 14),
            (32, 13, 15, "Creativity (AI)", 20),
            (35, 17, 45, "Emotions (AI)", 24),
            (39, 20, 0, "Select from Images Pathway (AI)", 18),
            (42, 12, 0, "Pour Paint", 15),
            (45, 16, 30, "AI Created Mandala (AI)", 25),
            (48, 10, 0, "Self (AI)", 22),
            (51, 19, 15, "Reflection Journals (AI)", 20),
            (54, 15, 0, "Journey (AI)", 24)
        };

        foreach (var s in p1MobileSchedule)
        {
            _sessions.Add(new PatientSession
            {
                PatientId = patient1Id,
                StartedAt = baseUtc.AddDays(-s.daysAgo).AddHours(s.hour).AddMinutes(s.minute),
                Platform = ReflectProductCatalog.PlatformMobile,
                ModuleName = s.module,
                DurationMinutes = s.duration,
                RecordingStatus = "Succeeded"
            });
        }

        // Patient 2 Sessions (Regular ongoing engagement across XR and Mobile)
        var p2XrSchedule = new (int daysAgo, int hour, int minute, string module, int duration)[]
        {
            (1, 10, 15, "Rhythmic Relaxation", 24),
            (3, 16, 0, "Color Psychology", 22),
            (5, 11, 30, "Containment", 20),
            (6, 14, 45, "Flow", 26),
            (8, 9, 0, "Self Compassion", 25),
            (9, 18, 15, "Catharsis", 30),
            (11, 15, 20, "Emotional Expression", 24),
            (12, 8, 45, "Portal to the Subconscious", 28),
            (14, 13, 30, "Present Self Inside", 22),
            (15, 17, 0, "Present Self Outside", 24),
            (17, 10, 0, "Personal Strengths", 20),
            (18, 19, 45, "Future Self", 26),
            (20, 12, 15, "Hope Tree", 25),
            (21, 16, 30, "Appreciation of Life", 22),
            (23, 9, 30, "Finding Balance", 24),
            (24, 14, 10, "New Pathways", 28),
            (26, 11, 0, "Training Tutorial", 18),
            (27, 18, 0, "Containment", 22),
            (29, 15, 45, "Emotional Expression", 26),
            (32, 10, 30, "Flow", 25),
            (34, 13, 0, "Personal Strengths", 20),
            (37, 17, 15, "Hope Tree", 24),
            (40, 11, 45, "Catharsis", 28),
            (43, 9, 15, "Rhythmic Relaxation", 22),
            (46, 16, 0, "Finding Balance", 25),
            (49, 14, 30, "Present Self Inside", 20),
            (52, 18, 45, "Color Psychology", 24),
            (55, 10, 0, "Self Compassion", 22)
        };

        foreach (var s in p2XrSchedule)
        {
            _sessions.Add(new PatientSession
            {
                PatientId = patient2Id,
                StartedAt = baseUtc.AddDays(-s.daysAgo).AddHours(s.hour).AddMinutes(s.minute),
                Platform = ReflectProductCatalog.PlatformXR,
                ModuleName = s.module,
                DurationMinutes = s.duration,
                RecordingStatus = "Succeeded"
            });
        }

        var p2MobileSchedule = new (int daysAgo, int hour, int minute, string module, int duration)[]
        {
            (1, 12, 0, "Finger Painting", 16),
            (2, 19, 30, "Floating Bubbles", 15),
            (3, 8, 45, "Breathing Mandala", 14),
            (5, 13, 15, "Pour Paint", 18),
            (6, 17, 0, "AI Created Mandala (AI)", 22),
            (7, 10, 30, "Photo Coloring (AI)", 20),
            (9, 14, 0, "Create Your Own Prompt (AI)", 25),
            (10, 20, 15, "Options Prompt Pathway (AI)", 18),
            (12, 11, 0, "Select from Images Pathway (AI)", 22),
            (13, 16, 45, "Reflection Journals (AI)", 20),
            (15, 9, 15, "Creativity (AI)", 24),
            (16, 15, 30, "Emotions (AI)", 22),
            (18, 12, 45, "Self (AI)", 20),
            (19, 18, 0, "Journey (AI)", 26),
            (21, 8, 15, "Floating Bubbles", 14),
            (22, 13, 30, "Reflection Journals (AI)", 18),
            (24, 17, 15, "Photo Coloring (AI)", 22),
            (25, 21, 30, "AI Created Mandala (AI)", 24),
            (28, 10, 0, "Create Your Own Prompt (AI)", 25),
            (31, 14, 15, "Options Prompt Pathway (AI)", 18),
            (33, 9, 30, "Select from Images Pathway (AI)", 20),
            (36, 16, 0, "Creativity (AI)", 22),
            (38, 11, 15, "Emotions (AI)", 24),
            (41, 19, 0, "Reflection Journals (AI)", 20),
            (44, 8, 45, "Finger Painting", 15),
            (47, 13, 0, "Pour Paint", 18),
            (50, 17, 30, "Self (AI)", 22),
            (53, 15, 15, "Journey (AI)", 25)
        };

        foreach (var s in p2MobileSchedule)
        {
            _sessions.Add(new PatientSession
            {
                PatientId = patient2Id,
                StartedAt = baseUtc.AddDays(-s.daysAgo).AddHours(s.hour).AddMinutes(s.minute),
                Platform = ReflectProductCatalog.PlatformMobile,
                ModuleName = s.module,
                DurationMinutes = s.duration,
                RecordingStatus = "Succeeded"
            });
        }

        // Patient 3: Historical sessions for a former patient (> 60 days ago)
        for (int i = 0; i < 6; i++)
        {
            _sessions.Add(new PatientSession
            {
                PatientId = patient3Id,
                StartedAt = baseUtc.AddDays(-80 + i * 5).AddHours(14),
                Platform = ReflectProductCatalog.PlatformMobile,
                ModuleName = "Floating Bubbles",
                DurationMinutes = 18,
                RecordingStatus = "Succeeded"
            });
        }

        // Patient 4: Unassigned patient (not accessible by DemoClinician)
        for (int i = 0; i < 5; i++)
        {
            _sessions.Add(new PatientSession
            {
                PatientId = patient4Id,
                StartedAt = baseUtc.AddDays(-48 + i * 6).AddHours(11),
                Platform = ReflectProductCatalog.PlatformXR,
                ModuleName = "Containment",
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
