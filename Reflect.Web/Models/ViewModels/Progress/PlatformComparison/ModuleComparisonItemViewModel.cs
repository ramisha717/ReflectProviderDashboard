namespace Reflect.Web.Models.ViewModels.Progress.PlatformComparison;

public class ModuleComparisonItemViewModel
{
    public string ModuleName { get; set; } = string.Empty;

    public string ProgramOrGroup { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;

    public int SessionCount { get; set; }

    public int TotalDurationMinutes { get; set; }

    public double PercentageOfPlatform { get; set; }
}
