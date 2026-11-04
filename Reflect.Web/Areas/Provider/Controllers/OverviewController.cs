
using Microsoft.AspNetCore.Mvc;
using Reflect.Domain.Repositories;
using Reflect.Web.Models.ViewModels.Overview.Index;
using Microsoft.AspNetCore.Authorization;
using Reflect.Domain.Services;
using Reflect.Web.Services;
using Reflect.Web.Messages;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
[Authorize(Policy = "ClinicianOnly")]
public class OverviewController : Controller
{
  private readonly IPatientRepository _patients;
  private readonly IPatientSessionRepository _sessions;
  private readonly ICurrentUserService _currentUser;
  private readonly IPatientAccessRepository _patientAccess;
  private readonly AttentionAcknowledgementService _acknowledgements;

  public OverviewController(
    IPatientRepository patients,
    IPatientSessionRepository sessions,
    ICurrentUserService currentUser,
    IPatientAccessRepository patientAccess,
    AttentionAcknowledgementService acknowledgements)
  {
    _patients = patients;
    _sessions = sessions;
    _currentUser = currentUser;
    _patientAccess = patientAccess;
    _acknowledgements = acknowledgements;
  }

  [HttpGet]
  public IActionResult IndexPage()
  {
      return RedirectToAction(nameof(Index));
  }

  [HttpGet]
  public async Task<IActionResult> AttentionReview(
    int? inactivityThresholdDays, int? frequencyDeclineThresholdPercent,
    CancellationToken cancellationToken)
  {
    var result = await BuildOverview(inactivityThresholdDays, frequencyDeclineThresholdPercent, cancellationToken);
    return result.Error ?? View("AttentionReview", result.Model);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> MarkReviewed(
    Guid patientId, string signalType,
    int? inactivityThresholdDays, int? frequencyDeclineThresholdPercent,
    CancellationToken cancellationToken)
  {
    if (patientId == Guid.Empty ||
        signalType is not ("Needs Review" or "Inactivity" or "Frequency Decline"))
        return BadRequest();
    var clinicianId = _currentUser.ClinicianId;
    if (!clinicianId.HasValue) return Unauthorized();
    if (!await _patientAccess.CanAccessPatientAsync(clinicianId.Value, patientId, cancellationToken))
        return Forbid();
    var result = await BuildOverview(inactivityThresholdDays, frequencyDeclineThresholdPercent, cancellationToken);
    if (result.Error is not null) return result.Error;
    if (!result.Model!.AttentionQueue.Any(row => row.PatientId == patientId && row.SignalType == signalType))
        return NotFound();

    if (signalType == "Needs Review")
        await _patients.SetReviewStatusAsync(patientId, "Reviewed", cancellationToken);
    else
    {
        var sessions = await _sessions.GetAllAsync(cancellationToken);
        var latest = sessions.Where(s => s.PatientId == patientId &&
                s.RecordingStatus == "Succeeded" && s.StartedAt <= DateTimeOffset.UtcNow)
            .Select(s => s.StartedAt).DefaultIfEmpty(DateTimeOffset.MinValue).Max();
        _acknowledgements.Acknowledge(clinicianId.Value, patientId, signalType, latest);
    }
    return RedirectToAction(nameof(AttentionReview), new {
        inactivityThresholdDays, frequencyDeclineThresholdPercent });
  }

  private async Task<(OverviewIndexViewModel Model, IActionResult Error)> BuildOverview(
    int? inactivityThresholdDays, int? frequencyDeclineThresholdPercent,
    CancellationToken cancellationToken)
  {
    var clinicianId = _currentUser.ClinicianId;
    if (!clinicianId.HasValue) return (null, Unauthorized());
    if (inactivityThresholdDays is < 1 || frequencyDeclineThresholdPercent is < 1 or > 100)
        return (null, BadRequest(ProviderMessages.InvalidAttentionThresholds));
    var vm = new OverviewIndexViewModel();
    await vm.PrepareData(_patients, _sessions, _patientAccess, _acknowledgements,
        clinicianId.Value, inactivityThresholdDays, frequencyDeclineThresholdPercent, cancellationToken);
    return (vm, null);
  }

  public async Task<IActionResult> Index(
    int? inactivityThresholdDays,
    int? frequencyDeclineThresholdPercent,
    CancellationToken cancellationToken)
  {
    var result = await BuildOverview(inactivityThresholdDays, frequencyDeclineThresholdPercent, cancellationToken);
    return result.Error ?? View(result.Model);
  }
}

