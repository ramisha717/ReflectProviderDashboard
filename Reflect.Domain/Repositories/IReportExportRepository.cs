
using Reflect.Domain.Entities;

namespace Reflect.Domain.Repositories;

public interface IReportExportRepository
{
    Task<IReadOnlyList<ReportExport>> GetByPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken);

    Task AddAsync(
        ReportExport reportExport,
        CancellationToken cancellationToken);
}
