namespace Reflect.Domain.Services;

public interface ISignedReportService
{
    byte[] CreateSignedPackage(byte[] reportBytes, string fileName);
    bool Verify(byte[] reportBytes, SignedReportManifest manifest, string fileName);
}
