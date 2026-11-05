
using System.Security.Cryptography;
using Reflect.Domain.Services;

namespace Reflect.Web.Services;

/// <summary>
/// Demo-only application signing identity. Production should use a managed key
/// service and persist signing-key versions for long-term verification.
/// </summary>
public sealed class ReportSigningService : IReportSigningService, IDisposable
{
    private readonly RSA _key;

    public string KeyId { get; }
    public string Algorithm => "RSASSA-PSS-SHA256";

    public ReportSigningService(
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _key = RSA.Create();

        var pem = configuration["ReportSigning:PrivateKeyPem"];

        if (string.IsNullOrWhiteSpace(pem))
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "Configure ReportSigning:PrivateKeyPem outside Development.");
            }

            _key.KeySize = 3072;
        }
        else
        {
            _key.ImportFromPem(pem);
        }

        var fingerprint = SHA256.HashData(
            _key.ExportSubjectPublicKeyInfo());

        KeyId = Convert.ToHexString(fingerprint).ToLowerInvariant();
    }

    public byte[] Sign(byte[] reportBytes)
    {
        ArgumentNullException.ThrowIfNull(reportBytes);

        return _key.SignData(
            reportBytes,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pss);
    }

    public bool Verify(byte[] reportBytes, byte[] signature)
    {
        ArgumentNullException.ThrowIfNull(reportBytes);
        ArgumentNullException.ThrowIfNull(signature);

        return _key.VerifyData(
            reportBytes,
            signature,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pss);
    }

    public string ExportPublicKeyPem()
    {
        return _key.ExportSubjectPublicKeyInfoPem();
    }

    public void Dispose()
    {
        _key.Dispose();
    }
}


