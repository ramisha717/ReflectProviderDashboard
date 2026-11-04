
using Reflect.Domain.Entities;
using Reflect.Domain.Enums;
using Reflect.Domain.Repositories;
using Reflect.Domain.Services;
using Reflect.Web.Messages;

namespace Reflect.Web.Models.ViewModels.Reports;

public class ReportRequest
{
    public Guid PatientId { get; set; }

    public ReportType ReportType { get; set; } =
        ReportType.EngagementTrends;

    public DateTimeOffset PeriodStart { get; set; } =
        DateTimeOffset.UtcNow.AddDays(-30);

    public DateTimeOffset PeriodEnd { get; set; } =
        DateTimeOffset.UtcNow;

    public string PlatformFilter { get; set; } = string.Empty;

    public bool IncludeSessionCountAndDuration { get; set; } = true;

    public bool IncludePlatformActivity { get; set; } = true;

    public bool IncludeModuleActivity { get; set; } = true;

    public bool IncludeSessionDetails { get; set; } = true;
}

public class ReportPreviewViewModel
{
    public Guid PatientId { get; set; }

    public string PatientCode { get; set; } = string.Empty;

    public ReportContent Report { get; set; } = new();

    public IReadOnlyList<ReportExport> RecentExports { get; set; } =
        Array.Empty<ReportExport>();
}

public class ReportsViewModel
{
    private readonly IPatientRepository _patients;
    private readonly IPatientSessionRepository _sessions;
    private readonly IReportExportRepository _exports;
    private readonly IReportGenerationService _generator;

    public ReportsViewModel(
        IPatientRepository patients,
        IPatientSessionRepository sessions,
        IReportExportRepository exports,
        IReportGenerationService generator)
    {
        _patients = patients;
        _sessions = sessions;
        _exports = exports;
        _generator = generator;
    }

    public async Task<ReportPreviewViewModel> GeneratePreviewAsync(
        ReportRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        var patient = await _patients.GetByIdAsync(
            request.PatientId,
            cancellationToken);

        if (patient is null)
            return null;

        var sessions = await _sessions.GetSessionsByPatientAsync(
            request.PatientId,
            cancellationToken);

        var report = _generator.GenerateReport(
            request.ReportType,
            patient,
            sessions,
            request.PeriodStart,
            request.PeriodEnd,
            request.PlatformFilter,
            request.IncludeSessionCountAndDuration,
            request.IncludePlatformActivity,
            request.IncludeModuleActivity,
            request.IncludeSessionDetails);

        var recentExports = await _exports.GetByPatientAsync(
            request.PatientId,
            cancellationToken);

        return new ReportPreviewViewModel
        {
            PatientId = patient.Id,
            PatientCode = patient.PatientCode,
            Report = report,
            RecentExports = recentExports
        };
    }

    public async Task<IReadOnlyList<ReportExport>> GetRecentExportsAsync(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdAsync(
            patientId,
            cancellationToken);

        if (patient is null)
            return null;

        return await _exports.GetByPatientAsync(
            patientId,
            cancellationToken);
    }

    
private static void ValidateRequest(ReportRequest request)
{
    if (request.PeriodStart > request.PeriodEnd)
    {
        throw new ArgumentException(
            ProviderMessages.InvalidReportingDateRange);
    }

    if (!Enum.IsDefined(request.ReportType) ||
        request.ReportType == ReportType.RtmStatus)
    {
        throw new ArgumentException(
            ProviderMessages.ReportTemplateUnavailable);
    }

    if (!string.IsNullOrWhiteSpace(request.PlatformFilter) &&
        request.PlatformFilter != "Reflect XR" &&
        request.PlatformFilter != "Reflect Mobile")
    {
        throw new ArgumentException(
            ProviderMessages.InvalidPlatformFilter);
    }
}

}
