
using Microsoft.AspNetCore.Mvc;
using Reflect.Domain.Repositories;
using Reflect.Web.Models.ViewModels.ClinicalResponse.Index;
using Reflect.Web.Models.ViewModels.ClinicalResponse.Create;
using Microsoft.AspNetCore.Authorization;
using Reflect.Domain.Services;
using Reflect.Web.Messages;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
[Authorize(Policy = "ClinicianOnly")]
public class ClinicalResponseController : Controller
{
    private readonly IPatientRepository _patients;
    private readonly IClinicalObservationRepository _observations;
    private readonly IPatientAuthorizationService _authorization;
    private readonly ICurrentUserService _currentUser;

    public ClinicalResponseController(
        IPatientRepository patients,
        IClinicalObservationRepository observations,
        IPatientAuthorizationService authorization,
        ICurrentUserService currentUser)
    {
        _patients = patients;
        _observations = observations;
        _authorization = authorization;
        _currentUser = currentUser;
    }

    [HttpGet]
    public IActionResult IndexPage()
    {
        return View("~/Areas/Provider/Views/Patients/ClinicalResponse.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        if (!await _authorization.CanAccessPatientAsync(
            patientId,
            cancellationToken))
        {
            return Forbid();
        }

        var viewModel = new ClinicalResponseIndexViewModel();

        await viewModel.PrepareData(
            _patients,
            _observations,
            patientId,
            cancellationToken);

        return Json(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ClinicalObservationCreateViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var clinicianId = _currentUser.ClinicianId;

        if (!clinicianId.HasValue)
        {
            return Unauthorized();
        }

        try
        {
            var observation = await viewModel.PrepareDataForSave(
                _patients,
                clinicianId.Value,
                cancellationToken);

            if (!await _authorization.CanAccessPatientAsync(
                observation.PatientId,
                cancellationToken))
            {
                return Forbid();
            }

            await _observations.AddAsync(
                observation,
                cancellationToken);

            return Json(new
            {
                observationId = observation.Id,
                patientId = observation.PatientId,
                message = ProviderMessages.ObservationCreated
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

