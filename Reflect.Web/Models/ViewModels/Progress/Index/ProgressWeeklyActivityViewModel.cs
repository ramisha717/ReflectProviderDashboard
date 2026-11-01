using Reflect.Domain.Entities;

namespace Reflect.Web.Models.ViewModels.Progress.Index;

public class ProgressWeeklyActivityViewModel
{
    public int WeekNumber { get; set; }

    public int ReflectXRSessions { get; set; }

    public int ReflectMobileSessions { get; set; }

    public static List<ProgressWeeklyActivityViewModel> FromSessions(
        List<PatientSession> sessions,
        DateTimeOffset start,
        int weekCount)
    {
        var result = new List<ProgressWeeklyActivityViewModel>();

        for (int i = 0; i < weekCount; i++)
        {
            DateTimeOffset weekStart = start.AddDays(i * 7);
            DateTimeOffset weekEnd = weekStart.AddDays(7);

            var weekSessions = sessions
                .Where(s => s.StartedAt >= weekStart &&
                            s.StartedAt < weekEnd);

            result.Add(new ProgressWeeklyActivityViewModel
            {
                WeekNumber = i + 1,

                ReflectXRSessions = weekSessions.Count(s =>
                    s.Platform == "Reflect XR"),

                ReflectMobileSessions = weekSessions.Count(s =>
                    s.Platform == "Reflect Mobile")
            });
        }

        return result;
    }
}