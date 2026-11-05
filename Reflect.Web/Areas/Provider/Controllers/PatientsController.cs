
using Microsoft.AspNetCore.Mvc;
using Reflect.Domain.Repositories;
using Reflect.Web.Models.ViewModels.Patients.Index;
using Reflect.Web.Models.ViewModels.Patients.Profile;
using Reflect.Web.Models.ViewModels.Patients.SessionDetails;
using Microsoft.AspNetCore.Authorization;
using Reflect.Domain.Services;
using Reflect.Domain.Entities;
using Reflect.Web.Messages;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
[Authorize(Policy = "ClinicianOnly")]
public class PatientsController : Controller
{
  private readonly IPatientRepository _patients;
  private readonly IPatientSessionRepository _sessions;
  private readonly ICurrentClinicianUserService _currentUser;
  private readonly IPatientAccessRepository _patientAccess;
  private readonly IPatientAuthorizationService _authorization;

  public PatientsController (
  IPatientRepository patients,
  IPatientSessionRepository sessions,
  ICurrentClinicianUserService currentUser,
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
  public async Task<IActionResult> SessionDetails(
    Guid sessionId, Guid patientId, CancellationToken cancellationToken)
  {
    if (sessionId == Guid.Empty || patientId == Guid.Empty) return NotFound();
    if (!await _authorization.CanAccessPatientAsync(patientId, cancellationToken))
      return Forbid();

    var model = new PatientSessionDetailsViewModel();
    if (!await model.PrepareData(sessionId, patientId, _patients, _sessions, cancellationToken))
      return NotFound();

    return View("SessionDetails", model);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> FlagSessionForReview(
    Guid sessionId, Guid patientId, CancellationToken cancellationToken)
  {
    if (sessionId == Guid.Empty || patientId == Guid.Empty) return NotFound();
    if (!await _authorization.CanAccessPatientAsync(patientId, cancellationToken))
      return Forbid();

    var model = new PatientSessionDetailsViewModel();
    if (!await model.PrepareData(sessionId, patientId, _patients, _sessions, cancellationToken))
      return NotFound();

    await _patients.SetReviewStatusAsync(patientId, "Needs Review", cancellationToken);
    TempData["SessionReviewMessage"] = ProviderMessages.PatientFlaggedForReview;
    return RedirectToAction(nameof(SessionDetails), new { sessionId, patientId });
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

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> AddDemo(
    [FromBody] AddDemoPatientRequest request,
    CancellationToken cancellationToken)
  {
    var clinicianId = _currentUser.ClinicianId;

    if (!clinicianId.HasValue)
        return Unauthorized();

    var patientCode = request.PatientCode?.Trim();

    if (string.IsNullOrWhiteSpace(patientCode))
        return BadRequest(ProviderMessages.PatientCodeRequired);

    if (patientCode.Length > 100)
        return BadRequest(ProviderMessages.PatientCodeTooLong);

    if (request.ReviewStatus != "Reviewed" &&
        request.ReviewStatus != "Needs Review")
        return BadRequest(ProviderMessages.InvalidReviewStatus);

    var patient = new Patient
    {
        Id = Guid.NewGuid(),
        PatientCode = patientCode,
        ReviewStatus = request.ReviewStatus,
        IsActive = false
    };

    try
    {
        await _patients.AddAsync(patient, cancellationToken);
    }
    catch (InvalidOperationException)
    {
        return Conflict(ProviderMessages.DuplicatePatientCode);
    }

    await _patientAccess.GrantAccessAsync(
        clinicianId.Value,
        patient.Id,
        cancellationToken);

    return Ok(new { patientId = patient.Id });
  }

  public sealed class AddDemoPatientRequest
  {
    public string PatientCode { get; set; }  = string.Empty;
    public string ReviewStatus { get; set; } = string.Empty;
  }
}

