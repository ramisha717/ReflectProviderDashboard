
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Reflect.Domain.Services;
using Reflect.Web.Extensions;

namespace Reflect.Web.Services;

public class ReportFileExportService : IReportFileExportService
{
    public ReportFileExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GeneratePdf(ReportContent report)
    {
        ArgumentNullException.ThrowIfNull(report);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Text(report.Title)
                    .FontSize(20)
                    .Bold();

                page.Content().Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Text(
                        $"Patient: {report.PatientCode}");

                    column.Item().Text(
                        $"Reporting period: " +
                        $"{report.PeriodStart.ToUsDateString()} to " +
                        $"{report.PeriodEnd.ToUsDateString()}");

                    column.Item().Text(
                        $"Platform: {report.PlatformFilter ?? "All Platforms"}");

                    foreach (var section in report.Sections)
                    {
                        column.Item()
                            .Text(section.Heading)
                            .FontSize(13)
                            .Bold();

                        foreach (var line in section.Lines)
                        {
                            column.Item().Text(line);
                        }
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                    });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateCsv(ReportContent report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var csv = new StringBuilder();

        AppendCsvRow(csv, "Report", report.Title);
        AppendCsvRow(csv, "Patient", report.PatientCode);
        AppendCsvRow(
            csv, "Period Start", report.PeriodStart.ToUsDateString());
        AppendCsvRow(
            csv, "Period End", report.PeriodEnd.ToUsDateString());
        AppendCsvRow(
            csv, "Platform", report.PlatformFilter ?? "All Platforms");

        csv.AppendLine();

        AppendCsvRow(csv, "Section", "Detail");

        foreach (var section in report.Sections)
        {
            foreach (var line in section.Lines)
            {
                AppendCsvRow(csv, section.Heading, line);
            }
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)
            .GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(csv.ToString()))
            .ToArray();
    }

    private static void AppendCsvRow(
        StringBuilder csv,
        string first,
        string second)
    {
        csv.Append(EscapeCsv(first));
        csv.Append(',');
        csv.Append(EscapeCsv(second));
        csv.AppendLine();
    }

    private static string EscapeCsv(string value)
    {
        // Prevent spreadsheet software from interpreting
        // untrusted report text as a formula.
        if (value.Length > 0 &&
            (value[0] == '=' ||
             value[0] == '+' ||
             value[0] == '-' ||
             value[0] == '@' ||
             value[0] == '\t' ||
             value[0] == '\r' ||
             value[0] == '\n'))
        {
            value = "'" + value;
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}

