namespace Reflect.Domain.Services;

public sealed class SignedReportManifest
{
    public int Version { get; set; }
    public string FileName { get; set; } = "";
    public string Algorithm { get; set; } = "";
    public string KeyId { get; set; } = "";
    public string Signature { get; set; } = "";
}