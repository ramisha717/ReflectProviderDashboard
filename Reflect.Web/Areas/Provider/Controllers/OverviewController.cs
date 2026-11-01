using Microsoft.AspNetCore.Mvc;
using Reflect.Domain.Repositories;
using Reflect.Web.Models.ViewModels.Overview.Index;
using Microsoft.AspNetCore.Authorization;
using Reflect.Domain.Services;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
[Authorize(Policy = "ClinicianOnly")]
public class OverviewController : Controller
{
  private readonly IPatientRepository _patients;
  private readonly IPatientSessionRepository _sessions;
  private readonly ICurrentUserService _currentUser;
  private readonly IPatientAccessRepository _patientAccess;

  public OverviewController(
    IPatientRepository patients,
    IPatientSessionRepository sessions,
    ICurrentUserService currentUser,
    IPatientAccessRepository patientAccess)
  {
    _patients = patients;
    _sessions = sessions;
    _currentUser = currentUser;
    _patientAccess = patientAccess;
  }

  [HttpGet]
  public IActionResult IndexPage()
  {
    return View("Index");
  }

  [HttpGet]
  public IActionResult AttentionReview()
  {
    return View("AttentionReview");
  }

  public async Task<IActionResult> Index(
    int? inactivityThresholdDays,
    int? frequencyDeclineThresholdPercent,
    CancellationToken cancellationToken)
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
    if (inactivityThresholdDays is < 1 || frequencyDeclineThresholdPercent is < 1 or > 100)
    {
      return BadRequest("Invalid attention queue thresholds.");
    }

    OverviewIndexViewModel viewModel = new();

    await viewModel.PrepareData(
      _patients,
      _sessions,
      _patientAccess,
      clinicianId.Value,
      inactivityThresholdDays,
      frequencyDeclineThresholdPercent,
      cancellationToken);

    return Json(viewModel);
  }
}
