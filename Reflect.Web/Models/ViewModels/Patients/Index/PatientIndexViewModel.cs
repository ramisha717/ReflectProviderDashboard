using Reflect.Domain.Repositories;

namespace Reflect.Web.Models.ViewModels.Patients.Index;


public class PatientIndexViewModel
{
  public List<PatientRowViewModel> Rows { get; set; } = new();

  public async Task PrepareData(
    IPatientRepository patientRepository,
    IPatientSessionRepository patientSessionRepository,
    CancellationToken cancellationToken
  )
  {
    var patients = await patientRepository.GetAllAsync(cancellationToken);

    var rows = new List<PatientRowViewModel>();

    foreach (var patient in patients){

      var sessions = await patientSessionRepository.GetSessionsByPatientAsync(patient.Id, cancellationToken);
      var row = PatientRowViewModel.FromEntity(patient, sessions);
      rows.Add(row);
    }

    Rows = rows;

    
  }
}