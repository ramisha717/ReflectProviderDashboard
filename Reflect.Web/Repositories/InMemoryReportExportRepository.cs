
using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;

namespace Reflect.Web.Repositories;

public class InMemoryReportExportRepository : IReportExportRepository
{
    private readonly List<ReportExport> _exports = new();
    private readonly object _lock = new();

    public Task<IReadOnlyList<ReportExport>> GetByPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<ReportExport> exports;

        lock (_lock)
        {
            exports = _exports
                .Where(e => !e.IsDeleted && e.PatientId == patientId)
                .OrderByDescending(e => e.ExportedAt)
                .ToList();
        }

        return Task.FromResult(exports);
    }

    public Task AddAsync(
        ReportExport reportExport,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentNullException.ThrowIfNull(reportExport);

        lock (_lock)
        {
            if (_exports.Any(e => e.Id == reportExport.Id))
            {
                throw new InvalidOperationException(
                    "A report export with this ID already exists.");
            }

            _exports.Add(reportExport);
        }

        return Task.CompletedTask;
    }
}
