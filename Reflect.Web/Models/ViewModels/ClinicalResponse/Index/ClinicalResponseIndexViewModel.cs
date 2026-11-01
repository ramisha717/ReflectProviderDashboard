using Reflect.Domain.Repositories;

namespace Reflect.Web.Models.ViewModels.ClinicalResponse.Index;

public class ClinicalResponseIndexViewModel
{
    public Guid PatientId { get; set; }

    public string PatientCode { get; set; } = string.Empty;

    public List<ClinicalObservationRowViewModel> RecentObservations { get; set; } = new();

    public async Task PrepareData(
        IPatientRepository patientsRepo,
        IClinicalObservationRepository observationsRepo,
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var patient = await patientsRepo.GetByIdAsync(
            patientId,
            cancellationToken);

        if (patient == null)
        {
            throw new KeyNotFoundException("Patient not found.");
        }

        PatientId = patient.Id;
        PatientCode = patient.PatientCode;

        var observations = await observationsRepo.GetByPatientAsync(
            patientId,
            cancellationToken);

        RecentObservations = observations
            .OrderByDescending(o => o.ObservedAt)
            .Select(ClinicalObservationRowViewModel.FromEntity)
            .ToList();
    }
}