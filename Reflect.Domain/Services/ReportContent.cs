
using Reflect.Domain.Enums;

namespace Reflect.Domain.Services;

public class ReportContent
{
    public ReportType ReportType { get; set; }

    public string PatientCode { get; set; } = string.Empty;

    public DateTimeOffset PeriodStart { get; set; }

    public DateTimeOffset PeriodEnd { get; set; }

    public string? PlatformFilter { get; set; }

    public string Title { get; set; } = string.Empty;

    public List<ReportSection> Sections { get; set; } = new();
}

public class ReportSection
{
    public string Heading { get; set; } = string.Empty;

    public List<string> Lines { get; set; } = new();
}
