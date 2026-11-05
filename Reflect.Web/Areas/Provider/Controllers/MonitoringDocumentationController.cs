
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;
using Reflect.Domain.Services;
using Reflect.Web.Models.ViewModels.MonitoringDocumentation;
using Reflect.Web.Messages;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
[Authorize(Policy = "ClinicianOnly")]
public class MonitoringDocumentationController : Controller
{
    private readonly IPatientRepository _patients;
    private readonly IPatientSessionRepository _sessions;
    private readonly IClinicalObservationRepository _observations;
    private readonly IMonitoringDraftRepository _drafts;
    private readonly ISummaryGenerationService _summaryService;
    private readonly ICurrentClinicianUserService _currentUser;
    private readonly IPatientAuthorizationService _authorization;

    public MonitoringDocumentationController(
        IPatientRepository patients,
        IPatientSessionRepository sessions,
        IClinicalObservationRepository observations,
        IMonitoringDraftRepository drafts,
        ISummaryGenerationService summaryService,
        ICurrentClinicianUserService currentUser,
        IPatientAuthorizationService authorization)
    {
        _patients = patients;
        _sessions = sessions;
        _observations = observations;
        _drafts = drafts;
        _summaryService = summaryService;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    private async Task<bool> CanAccessPatientCodeAsync(
        string patientCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(patientCode))
        {
            return false;
        }

        var patients = await _patients.GetAllAsync(cancellationToken);

        var patient = patients.FirstOrDefault(p =>
            string.Equals(
                p.PatientCode,
                patientCode,
                StringComparison.OrdinalIgnoreCase));

        if (patient == null)
        {
            return false;
        }

        return await _authorization.CanAccessPatientAsync(
            patient.Id,
            cancellationToken);
    }

    [HttpGet]
    public IActionResult IndexPage(string patientCode)
    {
        ViewBag.PatientCode = patientCode ?? string.Empty;
        return View("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string patientCode,
        CancellationToken cancellationToken,
        int? days = null)
    {
        if (Request.Headers["Accept"].ToString().Contains("text/html")
            && !Request.Query.ContainsKey("format"))
        {
            ViewBag.PatientCode = patientCode ?? string.Empty;
            return View("Index");
        }

        if (string.IsNullOrWhiteSpace(patientCode))
        {
            return BadRequest(new
            {
                message = ProviderMessages.SelectPatientForSummary
            });
        }

        if (days.HasValue && days.Value != 30 && days.Value != 60)
        {
            return BadRequest(new
            {
                message = ProviderMessages.InvalidMonitoringPeriod
            });
        }

        if (!await CanAccessPatientCodeAsync(
            patientCode,
            cancellationToken))
        {
            return Forbid();
        }

        var clinicianId = _currentUser.ClinicianId;

        if (!clinicianId.HasValue)
        {
            return Unauthorized();
        }

        var viewModel = new MonitoringDocumentationViewModel
        {
            Days = days ?? 30
        };

        try
        {
            await viewModel.PrepareData(
                patientCode,
                _patients,
                _sessions,
                _observations,
                _drafts,
                _summaryService,
                clinicianId.Value,
                cancellationToken);

            return Json(viewModel);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Regenerate(
        string patientCode,
        MonitoringDocumentationViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!await CanAccessPatientCodeAsync(
            patientCode,
            cancellationToken))
        {
            return Forbid();
        }

        var clinicianId = _currentUser.ClinicianId;

        if (!clinicianId.HasValue)
        {
            return Unauthorized();
        }

        if (viewModel.Days != 30 && viewModel.Days != 60)
        {
            return BadRequest(
                ProviderMessages.InvalidMonitoringPeriod);
        }

        string unsavedNotes = viewModel.Notes;

        try
        {
            await viewModel.PrepareData(
                patientCode,
                _patients,
                _sessions,
                _observations,
                _drafts,
                _summaryService,
                clinicianId.Value,
                cancellationToken);

            viewModel.Notes = unsavedNotes ?? string.Empty;

            return Json(viewModel);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDraft(
        string patientCode,
        [FromForm] string notes,
        CancellationToken cancellationToken)
    {
        if (!await CanAccessPatientCodeAsync(
            patientCode,
            cancellationToken))
        {
            return Forbid();
        }

        var clinicianId = _currentUser.ClinicianId;

        if (!clinicianId.HasValue)
        {
            return Unauthorized();
        }

        var viewModel = new MonitoringDocumentationViewModel();

        try
        {
            var (draft, isNew) = await viewModel.PrepareDataForSave(
                patientCode,
                notes,
                _patients,
                _drafts,
                clinicianId.Value,
                cancellationToken);

            if (isNew)
            {
                await _drafts.AddAsync(
                    draft,
                    cancellationToken);
            }
            else
            {
                await _drafts.UpdateAsync(
                    draft,
                    cancellationToken);
            }

            return Json(new
            {
                patientCode = viewModel.PatientCode,
                message = ProviderMessages.DraftSaved
            });
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkReviewed(
        string patientCode,
        [FromForm] string notes,
        CancellationToken cancellationToken)
    {
        if (!await CanAccessPatientCodeAsync(
            patientCode,
            cancellationToken))
        {
            return Forbid();
        }

        var clinicianId = _currentUser.ClinicianId;

        if (!clinicianId.HasValue)
        {
            return Unauthorized();
        }

        var viewModel = new MonitoringDocumentationViewModel();

        try
        {
            var (observation, draftId) =
                await viewModel.PrepareDataForReview(
                    patientCode,
                    notes,
                    _patients,
                    _drafts,
                    clinicianId.Value,
                    cancellationToken);

            if (observation != null)
            {
                await _observations.AddAsync(
                    observation,
                    cancellationToken);
            }

            await _patients.SetReviewStatusAsync(
                viewModel.PatientId,
                "Reviewed",
                cancellationToken);

            if (draftId.HasValue)
            {
                await _drafts.DeleteAsync(
                    draftId.Value,
                    cancellationToken);
            }

            return Json(new
            {
                patientCode = viewModel.PatientCode,
                status = "Reviewed",
                message = ProviderMessages.PatientMarkedReviewed
            });
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

