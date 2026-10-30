using Microsoft.AspNetCore.Mvc;
using Reflect.Domain.Repositories;
using Reflect.Web.Models.ViewModels.Patients.Index;
using Reflect.Web.Models.ViewModels.Patients.Profile;

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

  public async Task<IActionResult> Profile(
    Guid patientId,
    CancellationToken cancellationToken
  )
  {
    var viewModel = new PatientProfileViewModel();

    await viewModel.PrepareData(patientId, _patients, _sessions, cancellationToken);

    return Json(viewModel);
  }
    // GET: /Provider/Patients/IndexPage
    // Returns the Razor view; the view will fetch JSON data from the existing Index (JSON) endpoint.
    public IActionResult IndexPage()
    {
        // No model needed – the client script will load data via AJAX.
        return View("Index");
    }
}