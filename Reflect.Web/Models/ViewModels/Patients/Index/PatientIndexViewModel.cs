using Reflect.Domain.Repositories;

namespace Reflect.Web.Models.ViewModels.Patients.Index;


public class PatientIndexViewModel
{
  public List<PatientRowViewModel> Rows { get; set; } = new();

  public async Task PrepareData(
    IPatientRepository patientRepository,
    IPatientSessionRepository patientSessionRepository,
    IPatientAccessRepository patientAccessRepository,
    Guid clinicianId,
    CancellationToken cancellationToken
  )
  {
    var patients = await patientRepository.GetAllAsync(cancellationToken);

    var rows = new List<PatientRowViewModel>();

    foreach (var patient in patients){

      bool canAccess = await patientAccessRepository.CanAccessPatientAsync(
        clinicianId,
        patient.Id,
        cancellationToken);

      if (!canAccess)
      {
          continue;
      }

      var sessions = await patientSessionRepository.GetSessionsByPatientAsync(patient.Id, cancellationToken);
      var row = PatientRowViewModel.FromEntity(patient, sessions);
      rows.Add(row);
    }

    Rows = rows;

    
  }
}