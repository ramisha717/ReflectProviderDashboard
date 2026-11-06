namespace Reflect.Web.Models.ViewModels.Progress.PlatformComparison;

public class HourlyDistributionItemViewModel
{
    public int Hour { get; set; }

    public string DisplayLabel { get; set; } = string.Empty;

    public int TotalCount { get; set; }

    public Dictionary<string, int> CountsByCategory { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public int GetCount(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return 0;
        }

        return CountsByCategory.TryGetValue(category, out var count) ? count : 0;
    }
}
