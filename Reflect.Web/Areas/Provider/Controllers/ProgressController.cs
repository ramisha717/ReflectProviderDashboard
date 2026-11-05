
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Reflect.Domain.Repositories;
using Reflect.Domain.Services;
using Reflect.Web.Models.ViewModels.Progress.Index;
using Reflect.Web.Models.ViewModels.Progress.PlatformComparison;
using Reflect.Web.Messages;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
[Authorize(Policy = "ClinicianOnly")]
public class ProgressController : Controller
{
    private readonly IPatientSessionRepository _sessionsRepo;
    private readonly IPatientRepository _patientsRepo;
    private readonly IPatientAccessRepository _patientAccess;
    private readonly ICurrentClinicianUserService _currentUser;

    public ProgressController(
        IPatientSessionRepository sessionsRepo,
        IPatientRepository patientsRepo,
        IPatientAccessRepository patientAccess,
        ICurrentClinicianUserService currentUser)
    {
        _sessionsRepo = sessionsRepo;
        _patientsRepo = patientsRepo;
        _patientAccess = patientAccess;
        _currentUser = currentUser;
    }

    [HttpGet]
    public IActionResult IndexPage()
    {
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        var clinicianId = _currentUser.ClinicianId;

        if (!clinicianId.HasValue)
        {
            return Unauthorized();
        }

        if (days != 30 && days != 60)
        {
            return BadRequest(ProviderMessages.InvalidDayPeriod);
        }

        var viewModel = new ProgressIndexViewModel();

        await viewModel.PrepareData(
            _sessionsRepo,
            _patientsRepo,
            _patientAccess,
            clinicianId.Value,
            days,
            cancellationToken);

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> PlatformComparisonPage(
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        var clinicianId = _currentUser.ClinicianId;
        if (!clinicianId.HasValue) return Unauthorized();
        if (days != 30 && days != 60)
            return BadRequest(ProviderMessages.InvalidWeekPeriod);

        var model = new ProgressPlatformComparisonViewModel();
        await model.PrepareData(
            _sessionsRepo, _patientsRepo, _patientAccess,
            clinicianId.Value, days, cancellationToken);
        return View("PlatformComparison", model);
    }

    [HttpGet]
    public async Task<IActionResult> PlatformComparison(
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        var clinicianId = _currentUser.ClinicianId;

        if (!clinicianId.HasValue)
        {
            return Unauthorized();
        }

        if (days != 30 && days != 60)
        {
            return BadRequest(ProviderMessages.InvalidDayPeriod);
        }

        var viewModel = new ProgressPlatformComparisonViewModel();

        await viewModel.PrepareData(
            _sessionsRepo,
            _patientsRepo,
            _patientAccess,
            clinicianId.Value,
            days,
            cancellationToken);

        return Json(viewModel);
    }
}
