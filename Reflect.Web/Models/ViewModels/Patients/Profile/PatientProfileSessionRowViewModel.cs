
using Reflect.Domain.Entities;

namespace Reflect.Web.Models.ViewModels.Patients.Profile;

public class PatientProfileSessionRowViewModel
{
  public Guid SessionId { get; set; }
  public DateTimeOffset StartedAt { get; set; }
  public string Platform { get; set; } = string.Empty;
  public string ModuleName { get; set; } = string.Empty;
  public int DurationMinutes { get; set; }
  public string RecordingStatus { get; set; } = string.Empty;

  public static PatientProfileSessionRowViewModel FromEntity(
      PatientSession session)
  {
      return new PatientProfileSessionRowViewModel
      {
        SessionId = session.Id,
        StartedAt = session.StartedAt,
        Platform = session.Platform,
        ModuleName = session.ModuleName,
        DurationMinutes = session.DurationMinutes,
        RecordingStatus = session.RecordingStatus
      };
  }
}
