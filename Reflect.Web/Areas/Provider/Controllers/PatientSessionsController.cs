using Microsoft.AspNetCore.Mvc;
using Reflect.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Reflect.Domain.Services;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
[Authorize(Policy = "ClinicianOnly")]
public class PatientSessionsController : Controller
{
  private readonly IPatientSessionRepository _sessions;
  private readonly IPatientAuthorizationService _authorization;

  public PatientSessionsController (IPatientSessionRepository session, IPatientAuthorizationService authorization)
  {
    _sessions = session;
    _authorization = authorization;
  }

  public async Task<ActionResult> Index(CancellationToken cancellationToken, Guid patientId)
  {
    if (!await _authorization.CanAccessPatientAsync(
      patientId,
      cancellationToken))
    {
        return Forbid();
    }
    var sessions = await _sessions.GetSessionsByPatientAsync(patientId, cancellationToken);

    return Json(sessions);
  }
}