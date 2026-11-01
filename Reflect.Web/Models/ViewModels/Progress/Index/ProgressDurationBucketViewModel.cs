using Reflect.Domain.Entities;

namespace Reflect.Web.Models.ViewModels.Progress.Index;

public class ProgressDurationBucketViewModel
{
    public string Label { get; set; } = string.Empty;

    public int SessionCount { get; set; }

    public double Percentage { get; set; }

    public static List<ProgressDurationBucketViewModel> FromSessions(
        List<PatientSession> sessions)
    {
        int total = sessions.Count;

        var buckets = new List<ProgressDurationBucketViewModel>
        {
            new()
            {
                Label = "Under 5 min",
                SessionCount = sessions.Count(s => s.DurationMinutes < 5)
            },
            new()
            {
                Label = "5–15 min",
                SessionCount = sessions.Count(s =>
                    s.DurationMinutes >= 5 && s.DurationMinutes < 15)
            },
            new()
            {
                Label = "15–30 min",
                SessionCount = sessions.Count(s =>
                    s.DurationMinutes >= 15 && s.DurationMinutes < 30)
            },
            new()
            {
                Label = "30+ min",
                SessionCount = sessions.Count(s => s.DurationMinutes >= 30)
            }
        };

        foreach (var bucket in buckets)
        {
            bucket.Percentage = total == 0
                ? 0
                : Math.Round(100.0 * bucket.SessionCount / total, 1);
        }

        return buckets;
    }
}