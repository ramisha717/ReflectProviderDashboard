namespace Reflect.Domain.Services;

public interface IReportSigningService
{
    string KeyId { get; }
    string Algorithm { get; }
    byte[] Sign(byte[] reportBytes);
    bool Verify(byte[] reportBytes, byte[] signature);
    string ExportPublicKeyPem();
}
