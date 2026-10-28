using Reflect.Domain.Entities;

namespace Reflect.Web.Models.ViewModels.Overview.Index;

public class OverviewRecentActivityRowViewModel
{
  public Guid PatientId { get; set; }
  public string PatientCode { get; set; } = string.Empty;
  public Guid SessionId { get; set; }
  public DateTimeOffset StartedAt { get; set; }
  public string Platform { get; set; } = string.Empty;
  public int DurationMinutes { get; set; }
  public string ModuleName { get; set; } = string.Empty;

  public static OverviewRecentActivityRowViewModel FromEntity(
    Patient patient,
    PatientSession session
  )
  {
    return new OverviewRecentActivityRowViewModel
    {
      PatientId = patient.Id,
      PatientCode = patient.PatientCode,
      SessionId = session.Id,
      StartedAt = session.StartedAt,
      Platform = session.Platform,
      DurationMinutes = session.DurationMinutes,
      ModuleName = session.ModuleName

    };
  }


}