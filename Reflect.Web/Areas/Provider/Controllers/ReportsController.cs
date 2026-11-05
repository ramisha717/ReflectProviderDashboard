
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.IO.Compression;
using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;
using Reflect.Domain.Services;
using Reflect.Web.Models.ViewModels.Reports;
using Reflect.Web.Messages;

namespace Reflect.Web.Areas.Provider.Controllers;

[Area("Provider")]
[Route("Provider/[controller]")]
[Authorize(Policy = "ClinicianOnly")]
public class ReportsController : Controller
{
    private readonly ReportsViewModel _reports;
    private readonly ISignedReportService _signedReports;
    private readonly IReportFileExportService _fileExporter;
    private readonly IReportExportRepository _exportRepository;
    private readonly ICurrentClinicianUserService _currentUser;
    private readonly IPatientAuthorizationService _authorization;

    public ReportsController(
        ReportsViewModel reports,
        ISignedReportService signedReports,
        IReportFileExportService fileExporter,
        IReportExportRepository exportRepository,
        ICurrentClinicianUserService currentUser,
        IPatientAuthorizationService authorization)
    {
        _reports = reports;
        _signedReports = signedReports;
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(
        [FromBody] ReportRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest(ProviderMessages.ReportRequestRequired);

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
                return NotFound(ProviderMessages.PatientNotFound);

            return Ok(preview);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("Export")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Export(
        [FromBody] ReportRequest request,
        [FromQuery] string format,
        CancellationToken cancellationToken,
        [FromQuery] bool signed = false)
    {
        if (request is null)
            return BadRequest(ProviderMessages.ReportRequestRequired);

        format = format?.Trim().ToLowerInvariant() ?? "";

        if (format is not ("pdf" or "csv"))
            return BadRequest(ProviderMessages.InvalidReportFormat);

        var clinicianId = _currentUser.ClinicianId;

        if (!clinicianId.HasValue)
            return Unauthorized();

        if (!await _authorization.CanAccessPatientAsync(
            request.PatientId, cancellationToken))
        {
            return Forbid();
        }

        ReportPreviewViewModel preview;

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
            return NotFound(ProviderMessages.PatientNotFound);

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

        if (signed)
        {
            var package = _signedReports.CreateSignedPackage(fileBytes, fileName);
            return File(package, "application/zip", fileName + ".signed.zip");
        }

        return File(fileBytes, contentType, fileName);
    }

    
[HttpPost("VerifySignedPackage")]
[ValidateAntiForgeryToken]
[RequestSizeLimit(15_000_000)]
public async Task<IActionResult> VerifySignedPackage(
    IFormFile package,
    CancellationToken cancellationToken)
{
    if (package is null || package.Length == 0 ||
        package.Length > 10_000_000)
    {
        return BadRequest(new
        {
            message = "Select a signed report ZIP (10 MB maximum)."
        });
    }

    try
    {
        await using var upload = package.OpenReadStream();
        using var archive = new ZipArchive(upload, ZipArchiveMode.Read);

        // Ignore directory entries and macOS metadata.
        var entries = archive.Entries
            .Where(e =>
                !e.FullName.EndsWith("/", StringComparison.Ordinal) &&
                !e.FullName.StartsWith(
                    "__MACOSX/", StringComparison.OrdinalIgnoreCase) &&
                !e.Name.StartsWith("._", StringComparison.Ordinal) &&
                e.Name != ".DS_Store")
            .ToList();

        if (entries.Count != 3)
        {
            return BadRequest(new
            {
                message = "Invalid signed report package."
            });
        }

        // Find the manifest regardless of its parent directory.
        var manifests = entries
            .Where(e => e.Name == "signature.json")
            .ToList();

        if (manifests.Count != 1 ||
            manifests[0].Length > 16_384)
        {
            return BadRequest(new
            {
                message = "Invalid signature manifest."
            });
        }

        var manifestEntry = manifests[0];

        // All package files must share the same parent directory.
        var prefix = manifestEntry.FullName[
            ..^manifestEntry.Name.Length];

        SignedReportManifest manifest;

        using (var input = manifestEntry.Open())
        {
            manifest =
                await JsonSerializer.DeserializeAsync<SignedReportManifest>(
                    input,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    },
                    cancellationToken);
        }

        if (manifest is null ||
            string.IsNullOrWhiteSpace(manifest.FileName) ||
            manifest.FileName != Path.GetFileName(manifest.FileName) ||
            manifest.FileName.Contains('\\') ||
            !(manifest.FileName.EndsWith(
                ".pdf", StringComparison.OrdinalIgnoreCase) ||
              manifest.FileName.EndsWith(
                ".csv", StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(new
            {
                message = "Invalid report filename in manifest."
            });
        }

        var reportEntry = entries.SingleOrDefault(e =>
            e.FullName == prefix + manifest.FileName);

        var publicKeyEntry = entries.SingleOrDefault(e =>
            e.FullName == prefix + "public-key.pem");

        if (reportEntry is null ||
            publicKeyEntry is null ||
            reportEntry.Length > 10_000_000)
        {
            return BadRequest(new
            {
                message = "Report file is missing or too large."
            });
        }

        using var buffer = new MemoryStream();

        await using (var input = reportEntry.Open())
        {
            await input.CopyToAsync(buffer, cancellationToken);
        }

        if (buffer.Length > 10_000_000)
        {
            return BadRequest(new
            {
                message = "Report file is too large."
            });
        }

        // Verify the original report bytes using the trusted
        // server-side signing key, never the ZIP's public key.
        var valid = _signedReports.Verify(
            buffer.ToArray(),
            manifest,
            manifest.FileName);

        return Ok(new
        {
            valid,
            message = valid
                ? "Signature valid. Report unchanged."
                : "Signature invalid or signing key not trusted."
        });
    }
    catch (Exception ex) when (
        ex is InvalidDataException or
              JsonException or
              IOException or
              ArgumentException or
              InvalidOperationException)
    {
        return BadRequest(new
        {
            message = "Invalid signed report package."
        });
    }
}


    [HttpGet("RecentExports")]
    public async Task<IActionResult> RecentExports(
        [FromQuery] Guid patientId,
        CancellationToken cancellationToken)
    {
        if (patientId == Guid.Empty)
            return BadRequest(ProviderMessages.PatientIdRequired);

        if (!await _authorization.CanAccessPatientAsync(
            patientId, cancellationToken))
        {
            return Forbid();
        }

        var exports = await _reports.GetRecentExportsAsync(
            patientId, cancellationToken);

        if (exports is null)
            return NotFound(ProviderMessages.PatientNotFound);

        return Ok(exports);
    }
}
