
namespace Reflect.Domain.Entities;

public class ReportExport : BaseEntity
{
    public Guid PatientId { get; set; }

    public Guid ClinicianId { get; set; }

    public string ReportType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string Format { get; set; } = string.Empty;

    public DateTimeOffset PeriodStart { get; set; }

    public DateTimeOffset PeriodEnd { get; set; }

    public string PlatformFilter { get; set; } = string.Empty;

    public DateTimeOffset ExportedAt { get; set; } = DateTimeOffset.UtcNow;

    public string Status { get; set; } = "Completed";
}
