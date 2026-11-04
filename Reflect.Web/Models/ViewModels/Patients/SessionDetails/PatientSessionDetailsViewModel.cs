using Reflect.Domain.Repositories;

namespace Reflect.Web.Models.ViewModels.Patients.SessionDetails;

public sealed class PatientSessionDetailsViewModel
{
    public Guid PatientId { get; private set; }
    public Guid SessionId { get; private set; }
    public string PatientCode { get; private set; } = string.Empty;
    public string ReviewStatus { get; private set; } = string.Empty;
    public DateTimeOffset StartedAt { get; private set; }
    public string Platform { get; private set; } = string.Empty;
    public string ModuleName { get; private set; } = string.Empty;
    public int DurationMinutes { get; private set; }
    public string RecordingStatus { get; private set; } = string.Empty;

    public async Task<bool> PrepareData(
        Guid sessionId,
        Guid patientId,
        IPatientRepository patients,
        IPatientSessionRepository sessions,
        CancellationToken cancellationToken)
    {
        var patient = await patients.GetByIdAsync(patientId, cancellationToken);
        if (patient is null || patient.IsDeleted) return false;

        var records = await sessions.GetSessionsByPatientAsync(patientId, cancellationToken);
        var session = records.FirstOrDefault(s => s.Id == sessionId && !s.IsDeleted);
        if (session is null) return false;

        PatientId = patient.Id;
        SessionId = session.Id;
        PatientCode = patient.PatientCode;
        ReviewStatus = patient.ReviewStatus;
        StartedAt = session.StartedAt;
        Platform = session.Platform;
        ModuleName = session.ModuleName;
        DurationMinutes = session.DurationMinutes;
        RecordingStatus = session.RecordingStatus;
        return true;
    }
}