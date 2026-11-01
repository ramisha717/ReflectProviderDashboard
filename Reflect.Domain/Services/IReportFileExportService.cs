
namespace Reflect.Domain.Services;

public interface IReportFileExportService
{
    byte[] GeneratePdf(ReportContent report);

    byte[] GenerateCsv(ReportContent report);
}
