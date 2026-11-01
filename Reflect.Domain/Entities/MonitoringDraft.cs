
namespace Reflect.Domain.Entities;

public class MonitoringDraft : BaseEntity
{
  public Guid PatientId { get; set; }

  public Guid ClinicianId { get; set; }

  public string Notes { get; set; } = string.Empty;

  public DateTimeOffset LastModifiedAt { get; set; } = DateTimeOffset.UtcNow;
}
