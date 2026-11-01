using Microsoft.AspNetCore.Mvc;
using Reflect.Domain.Repositories;
using Reflect.Web.Models.ViewModels.Patients.Index;
using Reflect.Web.Models.ViewModels.Patients.Profile;
using Microsoft.AspNetCore.Authorization;
using Reflect.Domain.Services;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
[Authorize(Policy = "ClinicianOnly")]
public class PatientsController : Controller
{
  private readonly IPatientRepository _patients;
  private readonly IPatientSessionRepository _sessions;
  private readonly ICurrentUserService _currentUser;
  private readonly IPatientAccessRepository _patientAccess;
  private readonly IPatientAuthorizationService _authorization;

  public PatientsController (
  IPatientRepository patients,
  IPatientSessionRepository sessions,
  ICurrentUserService currentUser,
  IPatientAccessRepository patientAccess,
  IPatientAuthorizationService authorization)
  {
    _patients = patients;
    _sessions = sessions;
    _currentUser = currentUser;
    _patientAccess = patientAccess;
    _authorization = authorization;
  }

  [HttpGet]
  public IActionResult IndexPage()
  {
    return View("Index");
  }

  [HttpGet]
  public IActionResult SinceLastVisit(Guid? patientId)
  {
    ViewBag.PatientId = patientId;
    return View("SinceLastVisit");
  }

  [HttpGet]
  public IActionResult ClinicalNotes(Guid? patientId)
  {
    ViewBag.PatientId = patientId;
    return View("ClinicalNotes");
  }

  [HttpGet]
  public IActionResult SessionDetails(string? sessionId, Guid? patientId)
  {
    ViewBag.SessionId = sessionId ?? "S-008117";
    ViewBag.PatientId = patientId;
    return View("SessionDetails");
  }

  public async Task<IActionResult> Index(CancellationToken cancellationToken)
  {
    if (Request.Headers.Accept.ToString().Contains("text/html") && !Request.Query.ContainsKey("format"))
    {
      return View("Index");
    }

    var clinicianId = _currentUser.ClinicianId;

    if (!clinicianId.HasValue)
    {
        return Unauthorized();
    }
    var viewModel = new PatientIndexViewModel();

    await viewModel.PrepareData(
    _patients,
    _sessions,
    _patientAccess,
    clinicianId.Value,
    cancellationToken);

    return Json(viewModel);
  }

  public async Task<IActionResult> Profile(
    Guid patientId,
    CancellationToken cancellationToken
  )
  {
    if (!await _authorization.CanAccessPatientAsync(
    patientId,
    cancellationToken))
    {
    return Forbid();
    }

    var viewModel = new PatientProfileViewModel();

    await viewModel.PrepareData(patientId, _patients, _sessions, cancellationToken);

    return Json(viewModel);
  }
}
