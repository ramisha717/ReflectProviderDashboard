
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Reflect.Domain.Repositories;
using Reflect.Domain.Services;
using Reflect.Web.Models.ViewModels.PatientSessions.Index;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
[Authorize(Policy = "ClinicianOnly")]
public class PatientSessionHistoryController : Controller
{
    private readonly IPatientRepository _patients;
    private readonly IPatientSessionRepository _sessions;
    private readonly ICurrentUserService _currentUser;
    private readonly IPatientAccessRepository _patientAccess;

    public PatientSessionHistoryController(
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

    public async Task<IActionResult> Index(
        string? patientCode,
        string? platform,
        DateOnly? startDate,
        DateOnly? endDate,
        string? module,
        CancellationToken cancellationToken)
    {
        var clinicianId = _currentUser.ClinicianId;

        if (!clinicianId.HasValue)
        {
            return Unauthorized();
        }

        if (startDate.HasValue && endDate.HasValue &&
            startDate.Value > endDate.Value)
        {
            return BadRequest("Start date cannot be after end date.");
        }

        var viewModel = new PatientSessionsIndexViewModel();

        await viewModel.PrepareData(
            _patients,
            _sessions,
            _patientAccess,
            clinicianId.Value,
            patientCode,
            platform,
            startDate,
            endDate,
            module,
            cancellationToken);

        return Json(viewModel);
    }
}
