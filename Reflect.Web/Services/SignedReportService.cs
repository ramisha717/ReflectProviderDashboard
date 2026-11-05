
using System.IO.Compression;
using System.Text.Json;
using Reflect.Domain.Services;

namespace Reflect.Web.Services;

public sealed class SignedReportService : ISignedReportService
{
  private readonly IReportSigningService _signingService;

  public SignedReportService(IReportSigningService signingService)
  {
    _signingService = signingService;
  }

  public byte[] CreateSignedPackage(byte[] reportBytes, string fileName)
  {
    ArgumentNullException.ThrowIfNull(reportBytes);

    if (string.IsNullOrWhiteSpace(fileName))
        throw new ArgumentException("A filename is required.", nameof(fileName));

    var signature = _signingService.Sign(reportBytes);

    var manifest = new SignedReportManifest
    {
      Version = 1,
      FileName = fileName,
      Algorithm = _signingService.Algorithm,
      KeyId = _signingService.KeyId,
      Signature = Convert.ToBase64String(signature)
    };

    using var stream = new MemoryStream();

    using (var archive = new ZipArchive(
        stream, ZipArchiveMode.Create, leaveOpen: true))
    {
      WriteEntry(archive, fileName, reportBytes);

      WriteEntry(
          archive,
          "signature.json",
          JsonSerializer.SerializeToUtf8Bytes(manifest));

      WriteEntry(
          archive,
          "public-key.pem",
          System.Text.Encoding.UTF8.GetBytes(
              _signingService.ExportPublicKeyPem()));
    }

    return stream.ToArray();
  }

  public bool Verify(
    byte[] reportBytes,
    SignedReportManifest manifest,
    string fileName)
  {
    ArgumentNullException.ThrowIfNull(reportBytes);

    if (manifest is null ||
        manifest.Version != 1 ||
        !string.Equals(
            manifest.FileName,
            fileName,
            StringComparison.Ordinal) ||
        !string.Equals(
            manifest.KeyId,
            _signingService.KeyId,
            StringComparison.Ordinal) ||
        !string.Equals(
            manifest.Algorithm,
            _signingService.Algorithm,
            StringComparison.Ordinal) ||
        string.IsNullOrWhiteSpace(manifest.Signature))
    {
        return false;
    }

    try
    {
        var signature = Convert.FromBase64String(manifest.Signature);

        return _signingService.Verify(reportBytes, signature);
    }
    catch (FormatException)
    {
        return false;
    }
    catch (System.Security.Cryptography.CryptographicException)
    {
        return false;
    }
  }

  private static void WriteEntry(
      ZipArchive archive,
      string name,
      byte[] contents)
  {
      var entry = archive.CreateEntry(name, CompressionLevel.Optimal);

      using var destination = entry.Open();
      destination.Write(contents);
  }
}

