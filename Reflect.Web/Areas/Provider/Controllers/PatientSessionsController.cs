using Microsoft.AspNetCore.Mvc;
using Reflect.Domain.Repositories;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
public class PatientSessionsController : Controller
{
  private readonly IPatientSessionRepository _sessions;

  public PatientSessionsController (IPatientSessionRepository session)
  {
    _sessions = session;
  }

  public async Task<ActionResult> Index(CancellationToken cancellationToken, Guid patientId)
  {
    var sessions = await _sessions.GetSessionsByPatientAsync(patientId, cancellationToken);

    return Json(sessions);
  }
}