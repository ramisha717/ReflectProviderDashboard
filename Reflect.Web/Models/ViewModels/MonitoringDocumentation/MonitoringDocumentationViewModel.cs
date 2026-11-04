
using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;
using Reflect.Domain.Services;

namespace Reflect.Web.Models.ViewModels.MonitoringDocumentation;

public class MonitoringDocumentationViewModel
{
    public string PatientCode { get; set; } = string.Empty;

    public Guid PatientId { get; set; }

    public int Days { get; set; } = 30;

    public bool IncludeSessionCountAndDuration { get; set; } = true;

    public bool IncludePlatformActivity { get; set; } = true;

    public bool IncludeModuleActivity { get; set; } = true;

    public bool IncludeClinicianObservations { get; set; } = true;

    public string GeneratedSummary { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public async Task PrepareData(
        string patientCode,
        IPatientRepository patientsRepo,
        IPatientSessionRepository sessionsRepo,
        IClinicalObservationRepository observationsRepo,
        IMonitoringDraftRepository draftsRepo,
        ISummaryGenerationService summaryService,
        Guid clinicianId,
        CancellationToken cancellationToken)
    {
        if (Days != 30 && Days != 60)
        {
            throw new ArgumentException(
                "Reporting period must be 30 or 60 days.");
        }

        var patients = await patientsRepo.GetAllAsync(
            cancellationToken);

        var patient = patients.FirstOrDefault(p =>
            string.Equals(
                p.PatientCode,
                patientCode,
                StringComparison.OrdinalIgnoreCase));

        if (patient == null)
        {
            throw new EntityNotFoundException("Patient not found.");
        }

        PatientId = patient.Id;
        PatientCode = patient.PatientCode;

        var now = DateTimeOffset.UtcNow;
        var periodStart = now.AddDays(-Days);

        var allSessions = await sessionsRepo.GetSessionsByPatientAsync(
            PatientId,
            cancellationToken);

        var sessions = allSessions
            .Where(s =>
                s.StartedAt >= periodStart &&
                s.StartedAt <= now &&
                s.RecordingStatus == "Succeeded")
            .ToList();

        var allObservations = await observationsRepo.GetByPatientAsync(
            PatientId,
            cancellationToken);

        var clinicalObservations = allObservations
            .Where(o =>
                o.ObservedAt >= periodStart &&
                o.ObservedAt <= now)
            .ToList();

        GeneratedSummary = summaryService.GenerateSummary(
            sessions,
            IncludeSessionCountAndDuration,
            IncludePlatformActivity,
            IncludeModuleActivity,
            IncludeClinicianObservations,
            clinicalObservations);

        var draft = await draftsRepo.GetByPatientAndClinicianAsync(
            PatientId,
            clinicianId,
            cancellationToken);

        Notes = draft?.Notes ?? string.Empty;
    }

    public async Task<(MonitoringDraft Draft, bool IsNew)> PrepareDataForSave(
        string patientCode,
        string notes,
        IPatientRepository patientsRepo,
        IMonitoringDraftRepository draftsRepo,
        Guid clinicianId,
        CancellationToken cancellationToken)
    {
        var patients = await patientsRepo.GetAllAsync(
            cancellationToken);

        var patient = patients.FirstOrDefault(p =>
            string.Equals(
                p.PatientCode,
                patientCode,
                StringComparison.OrdinalIgnoreCase));

        if (patient == null)
        {
            throw new EntityNotFoundException("Patient not found.");
        }

        PatientId = patient.Id;
        PatientCode = patient.PatientCode;
        Notes = notes ?? string.Empty;

        var existingDraft = await draftsRepo.GetByPatientAndClinicianAsync(
            PatientId,
            clinicianId,
            cancellationToken);

        var draft = new MonitoringDraft
        {
            PatientId = PatientId,
            ClinicianId = clinicianId,
            Notes = Notes,
            LastModifiedAt = DateTimeOffset.UtcNow
        };

        if (existingDraft != null)
        {
            draft.Id = existingDraft.Id;
        }

        return (draft, existingDraft == null);
    }

    public async Task<(ClinicalObservation Observation, Guid? DraftId)>
        PrepareDataForReview(
            string patientCode,
            string notes,
            IPatientRepository patientsRepo,
            IMonitoringDraftRepository draftsRepo,
            Guid clinicianId,
            CancellationToken cancellationToken)
    {
        var patients = await patientsRepo.GetAllAsync(
            cancellationToken);

        var patient = patients.FirstOrDefault(p =>
            string.Equals(
                p.PatientCode,
                patientCode,
                StringComparison.OrdinalIgnoreCase));

        if (patient == null)
        {
            throw new EntityNotFoundException("Patient not found.");
        }

        PatientId = patient.Id;
        PatientCode = patient.PatientCode;
        Notes = notes ?? string.Empty;

        ClinicalObservation observation = null;

        if (!string.IsNullOrWhiteSpace(Notes))
        {
            observation = new ClinicalObservation
            {
                PatientId = PatientId,
                ClinicianId = clinicianId,
                PatientSessionId = null,
                ObservedAt = DateTimeOffset.UtcNow,
                ObservationType = "Session Review",
                Notes = Notes
            };
        }

        var draft = await draftsRepo.GetByPatientAndClinicianAsync(
            PatientId,
            clinicianId,
            cancellationToken);

        return (observation, draft?.Id);
    }
}

