using Microsoft.AspNetCore.Mvc;
using Reflect.Domain.Repositories;
using Reflect.Web.Models.ViewModels.Patients.Index;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
public class PatientsController : Controller
{
  private readonly IPatientRepository _patients;
  private readonly IPatientSessionRepository _sessions;

  public PatientsController (IPatientRepository patients, IPatientSessionRepository sessions)
  {
    _patients = patients;
    _sessions = sessions;
  }

  public async Task<IActionResult> Index(CancellationToken cancellationToken)
  {
    var viewModel = new PatientIndexViewModel();

    await viewModel.PrepareData(_patients, _sessions, cancellationToken);

    return Json(viewModel);
  }
}