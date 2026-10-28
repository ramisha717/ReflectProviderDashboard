using Microsoft.AspNetCore.Mvc;
using Reflect.Domain.Repositories;
using Reflect.Web.Models.ViewModels.Overview.Index;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
public class OverviewController : Controller
{
  private readonly IPatientRepository _patients;
  private readonly IPatientSessionRepository _sessions;

  public OverviewController(
    IPatientRepository patients,
    IPatientSessionRepository sessions)
  {
    _patients = patients;
    _sessions = sessions;
  }

  
  public async Task<IActionResult> Index(
    int? inactivityThresholdDays,
    int? frequencyDeclineThresholdPercent,
    CancellationToken cancellationToken)
  {
    if (inactivityThresholdDays is < 1 || frequencyDeclineThresholdPercent is < 1 or > 100)
    {
      return BadRequest("Invalid attention queue thresholds.");
    }

    OverviewIndexViewModel viewModel = new();

    await viewModel.PrepareData(
      _patients,
      _sessions,
      inactivityThresholdDays,
      frequencyDeclineThresholdPercent,
      cancellationToken);

    return Json(viewModel);
  }

}