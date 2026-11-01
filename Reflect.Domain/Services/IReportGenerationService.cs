
using Reflect.Domain.Entities;
using Reflect.Domain.Enums;

namespace Reflect.Domain.Services;


public interface IReportGenerationService
{
  ReportContent GenerateReport(
    ReportType reportType,
    Patient patient,
    IReadOnlyList<PatientSession> sessions,
    DateTimeOffset periodStart,
    DateTimeOffset periodEnd,
    string? platformFilter,
    bool includeSessionCountAndDuration,
    bool includePlatformActivity,
    bool includeModuleActivity,
    bool includeSessionDetails);
}
