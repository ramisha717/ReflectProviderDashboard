
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;
using Reflect.Domain.Services;
using Reflect.Web.Models.ViewModels.Reports;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
[Route("Provider/[controller]")]
[Authorize(Policy = "ClinicianOnly")]
public class ReportsController : Controller
{
    private readonly ReportsViewModel _reports;
    private readonly IReportFileExportService _fileExporter;
    private readonly IReportExportRepository _exportRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IPatientAuthorizationService _authorization;

    public ReportsController(
        ReportsViewModel reports,
        IReportFileExportService fileExporter,
        IReportExportRepository exportRepository,
        ICurrentUserService currentUser,
        IPatientAuthorizationService authorization)
    {
        _reports = reports;
        _fileExporter = fileExporter;
        _exportRepository = exportRepository;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    [HttpGet("IndexPage")]
    public IActionResult Index()
    {
        return View("Index");
    }

    [HttpPost("Preview")]
    public async Task<IActionResult> Preview(
        [FromBody] ReportRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest("A report request is required.");

        if (!await _authorization.CanAccessPatientAsync(
            request.PatientId, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            var preview = await _reports.GeneratePreviewAsync(
                request, cancellationToken);

            if (preview is null)
                return NotFound("Patient not found.");

            return Ok(preview);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("Export")]
    public async Task<IActionResult> Export(
        [FromBody] ReportRequest request,
        [FromQuery] string format,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest("A report request is required.");

        format = format?.Trim().ToLowerInvariant() ?? "";

        if (format is not ("pdf" or "csv"))
            return BadRequest("Format must be pdf or csv.");

        var clinicianId = _currentUser.ClinicianId;

        if (!clinicianId.HasValue)
            return Unauthorized();

        if (!await _authorization.CanAccessPatientAsync(
            request.PatientId, cancellationToken))
        {
            return Forbid();
        }

        ReportPreviewViewModel? preview;

        try
        {
            preview = await _reports.GeneratePreviewAsync(
                request, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        if (preview is null)
            return NotFound("Patient not found.");

        var report = preview.Report;

        byte[] fileBytes;
        string contentType;

        if (format == "pdf")
        {
            fileBytes = _fileExporter.GeneratePdf(report);
            contentType = "application/pdf";
        }
        else
        {
            fileBytes = _fileExporter.GenerateCsv(report);
            contentType = "text/csv; charset=utf-8";
        }

        var fileName =
            $"{report.ReportType}_{report.PatientCode}_" +
            $"{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}.{format}";

        var export = new ReportExport
        {
            PatientId = request.PatientId,
            ClinicianId = clinicianId.Value,
            ReportType = report.ReportType.ToString(),
            FileName = fileName,
            Format = format.ToUpperInvariant(),
            PeriodStart = report.PeriodStart,
            PeriodEnd = report.PeriodEnd,
            PlatformFilter = report.PlatformFilter ?? "All Platforms",
            ExportedAt = DateTimeOffset.UtcNow,
            Status = "Completed"
        };

        await _exportRepository.AddAsync(
            export, cancellationToken);

        return File(fileBytes, contentType, fileName);
    }

    [HttpGet("RecentExports")]
    public async Task<IActionResult> RecentExports(
        [FromQuery] Guid patientId,
        CancellationToken cancellationToken)
    {
        if (patientId == Guid.Empty)
            return BadRequest("A patient ID is required.");

        if (!await _authorization.CanAccessPatientAsync(
            patientId, cancellationToken))
        {
            return Forbid();
        }

        var exports = await _reports.GetRecentExportsAsync(
            patientId, cancellationToken);

        if (exports is null)
            return NotFound("Patient not found.");

        return Ok(exports);
    }
}
