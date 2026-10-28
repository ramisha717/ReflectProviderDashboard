
namespace Reflect.Web.Models.ViewModels.Overview.Index;

public class OverviewAttentionRowViewModel
{
    public Guid PatientId { get; set; }

    public string PatientCode { get; set; } = string.Empty;

    public string SignalType { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;
}
