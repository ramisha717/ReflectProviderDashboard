namespace Reflect.Domain.Entities;

public class PatientSession: BaseEntity
{
  public Guid PatientId { get; set; }
  public DateTimeOffset StartedAt { get; set; }
  public string Platform { get; set; } = string.Empty;
  public string ModuleName { get; set; } = string.Empty;
  public int DurationMinutes { get; set; }
  public string RecordingStatus { get; set; } = string.Empty;

}