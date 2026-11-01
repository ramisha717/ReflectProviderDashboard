using Reflect.Domain.Entities;

namespace Reflect.Web.Models.ViewModels.PatientSessions.Index;

public class PatientSessionHistoryRowViewModel
{
    public Guid SessionId { get; set; }

    public Guid PatientId { get; set; }

    public string PatientCode { get; set; } = string.Empty;

    public DateTimeOffset StartedAt { get; set; }

    public string Platform { get; set; } = string.Empty;

    public string ModuleName { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public string RecordingStatus { get; set; } = string.Empty;

    public static PatientSessionHistoryRowViewModel FromEntity(
        Patient patient,
        PatientSession session)
    {
        return new PatientSessionHistoryRowViewModel
        {
            SessionId = session.Id,
            PatientId = patient.Id,
            PatientCode = patient.PatientCode,
            StartedAt = session.StartedAt,
            Platform = session.Platform,
            ModuleName = session.ModuleName,
            DurationMinutes = session.DurationMinutes,
            RecordingStatus = session.RecordingStatus
        };
    }
}