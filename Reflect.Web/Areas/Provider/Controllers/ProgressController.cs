using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Reflect.Domain.Repositories;
using Reflect.Domain.Services;
using Reflect.Web.Models.ViewModels.Progress.Index;
using Reflect.Web.Models.ViewModels.Progress.PlatformComparison;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
[Authorize(Policy = "ClinicianOnly")]
public class ProgressController : Controller
{
    private readonly IPatientSessionRepository _sessionsRepo;
    private readonly IPatientRepository _patientsRepo;
    private readonly IPatientAccessRepository _patientAccess;
    private readonly ICurrentUserService _currentUser;

    public ProgressController(
        IPatientSessionRepository sessionsRepo,
        IPatientRepository patientsRepo,
        IPatientAccessRepository patientAccess,
        ICurrentUserService currentUser)
    {
        _sessionsRepo = sessionsRepo;
        _patientsRepo = patientsRepo;
        _patientAccess = patientAccess;
        _currentUser = currentUser;
    }

    [HttpGet]
    public IActionResult IndexPage()
    {
        return View("Index");
    }

    public async Task<IActionResult> Index(
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        // If regular browser navigation requested the UI
        if (Request.Headers["Accept"].ToString().Contains("text/html") && !Request.Query.ContainsKey("format"))
        {
            return View("Index");
        }

        var clinicianId = _currentUser.ClinicianId;

        if (!clinicianId.HasValue)
        {
            return Unauthorized();
        }

        if (days != 30 && days != 60)
        {
            return BadRequest("Only 30 or 60 days are supported.");
        }

        var viewModel = new ProgressIndexViewModel();

        await viewModel.PrepareData(
            _sessionsRepo,
            _patientsRepo,
            _patientAccess,
            clinicianId.Value,
            days,
            cancellationToken);

        return Json(viewModel);
    }

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
            return BadRequest("Only 30 or 60 days are supported.");
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
