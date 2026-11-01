using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;

namespace Reflect.Web.Models.ViewModels.ClinicalResponse.Create;

public class ClinicalObservationCreateViewModel
{
    public string PatientCode { get; set; } = string.Empty;

    public Guid? PatientSessionId { get; set; }

    public string ObservationType { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public async Task<ClinicalObservation> PrepareDataForSave(
        IPatientRepository patientsRepo,
        Guid clinicianId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(PatientCode))
        {
            throw new ArgumentException("A patient code is required.");
        }

        if (string.IsNullOrWhiteSpace(ObservationType))
        {
            throw new ArgumentException("Observation type is required.");
        }

        if (string.IsNullOrWhiteSpace(Notes))
        {
            throw new ArgumentException("Observation notes are required.");
        }

        var patients = await patientsRepo.GetAllAsync(cancellationToken);

        var patient = patients.FirstOrDefault(p =>
            p.PatientCode == PatientCode.Trim());

        if (patient == null)
        {
            throw new ArgumentException("Patient code not found.");
        }

        if (clinicianId == Guid.Empty)
        {
            throw new ArgumentException("A valid clinician ID is required.");
        }

        return new ClinicalObservation
        {
            PatientId = patient.Id,
            PatientSessionId = PatientSessionId,
            ClinicianId = clinicianId,
            ObservedAt = DateTimeOffset.UtcNow,
            ObservationType = ObservationType.Trim(),
            Notes = Notes.Trim()
        };
    }
}
