using Reflect.Domain.Entities;

namespace Reflect.Web.Models.ViewModels.Patients.Index;

public class PatientRowViewModel
{
    public Guid PatientId { get; set; }
    public string PatientCode { get; set; } = string.Empty;
    public DateTimeOffset? LastActive { get; set; }
    public int NumOfSessions { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Trend { get; set; } = string.Empty;
    public string ReviewStatus { get; set; } = string.Empty;

    public static PatientRowViewModel FromEntity(Patient patient, IReadOnlyList<PatientSession> sessions)
    {
        var successfulSessions = sessions
            .Where(session => session.RecordingStatus == "Succeeded")
            .OrderByDescending(session => session.StartedAt)
            .ToList();

        var lastSession = successfulSessions.FirstOrDefault();
        var now = DateTimeOffset.UtcNow;
        var thisMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        var thisMonthCount = successfulSessions.Count(s => s.StartedAt >= thisMonthStart && s.StartedAt <= now);
        var lastMonthCount = successfulSessions.Count(s => s.StartedAt >= lastMonthStart && s.StartedAt < thisMonthStart);

        var trend = thisMonthCount > lastMonthCount ? "Increased"
            : thisMonthCount < lastMonthCount ? "Decreased"
            : "Stable";

        return new PatientRowViewModel
        {
            PatientId = patient.Id,
            PatientCode = patient.PatientCode,
            LastActive = lastSession?.StartedAt,
            NumOfSessions = successfulSessions.Count,
            Platform = lastSession?.Platform ?? string.Empty,
            Trend = trend,
            ReviewStatus = patient.ReviewStatus
        };
    }
}


