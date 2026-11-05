using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Reflect.Domain.Services;
using Reflect.Web.Services;
using Xunit;

namespace Reflect.Tests;

public sealed class ReportSigningTests
{
    private static ReportSigningService NewKey() => new(
        new ConfigurationBuilder().Build(), new DevelopmentEnvironment());

    [Fact]
    public void SignedPackageContainsReportManifestAndPublicKey()
    {
        using var signer = NewKey();
        var service = new SignedReportService(signer);
        var original = Encoding.UTF8.GetBytes("patient,session\nA,1\n");
        using var archive = new ZipArchive(new MemoryStream(
            service.CreateSignedPackage(original, "report.csv")), ZipArchiveMode.Read);

        Assert.Equal(3, archive.Entries.Count);
        Assert.NotNull(archive.GetEntry("report.csv"));
        Assert.NotNull(archive.GetEntry("signature.json"));
        Assert.NotNull(archive.GetEntry("public-key.pem"));
        var manifest = ReadManifest(archive);
        Assert.Equal(1, manifest.Version);
        Assert.Equal("report.csv", manifest.FileName);
        Assert.Equal("RSASSA-PSS-SHA256", manifest.Algorithm);
        Assert.Equal(signer.KeyId, manifest.KeyId);
        Assert.True(service.Verify(original, manifest, "report.csv"));
    }

    [Fact]
    public void RepackingUnchangedReportDoesNotInvalidateSignature()
    {
        using var signer = NewKey();
        var service = new SignedReportService(signer);
        var report = Encoding.UTF8.GetBytes("unchanged report");
        var zip = service.CreateSignedPackage(report, "report.pdf");
        using var originalArchive = new ZipArchive(new MemoryStream(zip), ZipArchiveMode.Read);
        var manifest = ReadManifest(originalArchive);
        using var repackedStream = new MemoryStream();
        using (var repacked = new ZipArchive(repackedStream, ZipArchiveMode.Create, true))
        {
            foreach (var entry in originalArchive.Entries)
            {
                var newEntry = repacked.CreateEntry("enclosing-folder/" + entry.Name,
                    CompressionLevel.NoCompression);
                using var source = entry.Open();
                using var destination = newEntry.Open();
                source.CopyTo(destination);
            }
        }
        using var repackedArchive = new ZipArchive(
            new MemoryStream(repackedStream.ToArray()), ZipArchiveMode.Read);
        var repackedManifestEntry = repackedArchive.GetEntry("enclosing-folder/signature.json");
        Assert.NotNull(repackedManifestEntry);
        using var reportStream = repackedArchive.GetEntry("enclosing-folder/report.pdf").Open();
        using var bytes = new MemoryStream();
        reportStream.CopyTo(bytes);
        Assert.True(service.Verify(bytes.ToArray(), manifest, "report.pdf"));
    }

    [Fact]
    public void ModifiedReportAndWrongFileNameAreRejected()
    {
        using var signer = NewKey();
        var service = new SignedReportService(signer);
        var original = Encoding.UTF8.GetBytes("original");
        using var archive = new ZipArchive(new MemoryStream(
            service.CreateSignedPackage(original, "report.csv")), ZipArchiveMode.Read);
        var manifest = ReadManifest(archive);
        Assert.False(service.Verify(Encoding.UTF8.GetBytes("modified"), manifest, "report.csv"));
        Assert.False(service.Verify(original, manifest, "another.csv"));
    }

    [Fact]
    public void TamperedManifestIsRejected()
    {
        using var signer = NewKey();
        var service = new SignedReportService(signer);
        var original = Encoding.UTF8.GetBytes("report");
        using var archive = new ZipArchive(new MemoryStream(
            service.CreateSignedPackage(original, "report.csv")), ZipArchiveMode.Read);
        var manifest = ReadManifest(archive);
        manifest.Signature = "not base64!";
        Assert.False(service.Verify(original, manifest, "report.csv"));
        manifest.Signature = Convert.ToBase64String(new byte[384]);
        manifest.KeyId = "untrusted";
        Assert.False(service.Verify(original, manifest, "report.csv"));
        manifest.KeyId = signer.KeyId;
        manifest.Version = 2;
        Assert.False(service.Verify(original, manifest, "report.csv"));
        manifest.Version = 1;
        manifest.Algorithm = "unsupported";
        Assert.False(service.Verify(original, manifest, "report.csv"));
    }

    [Fact]
    public void SignaturesFromDifferentApplicationKeysAreNotTrusted()
    {
        using var first = NewKey();
        using var second = NewKey();
        var report = Encoding.UTF8.GetBytes("same bytes");
        using var archive = new ZipArchive(new MemoryStream(
            new SignedReportService(first).CreateSignedPackage(report, "report.csv")),
            ZipArchiveMode.Read);
        Assert.False(new SignedReportService(second).Verify(
            report, ReadManifest(archive), "report.csv"));
    }

    [Fact]
    public void SigningRequiresKeyOutsideDevelopment()
    {
        Assert.Throws<InvalidOperationException>(() => new ReportSigningService(
            new ConfigurationBuilder().Build(),
            new DevelopmentEnvironment { EnvironmentName = "Production" }));
    }

    private static SignedReportManifest ReadManifest(ZipArchive archive)
    {
        using var stream = archive.GetEntry("signature.json").Open();
        return JsonSerializer.Deserialize<SignedReportManifest>(stream);
    }

    private sealed class DevelopmentEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Reflect.Tests";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
