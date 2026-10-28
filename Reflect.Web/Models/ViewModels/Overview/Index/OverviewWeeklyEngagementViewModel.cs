
using Reflect.Domain.Entities;

namespace Reflect.Web.Models.ViewModels.Overview.Index;

public class OverviewWeeklyEngagementViewModel
{
  public DateTimeOffset WeekStart { get; set; }
  public int ReflectXRMinutes { get; set; }
  public int ReflectMobileMinutes { get; set; }

  public static List<OverviewWeeklyEngagementViewModel> FromSessions(
      IReadOnlyList<PatientSession> sessions)
  {
    TimeZoneInfo easternTimeZone =
      TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

    DateTimeOffset nowEastern =
      TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, easternTimeZone);

    DateTime currentDate = nowEastern.Date;

    int daysSinceMonday =
      ((int)currentDate.DayOfWeek + 6) % 7;

    DateTime currentWeekStart =
      currentDate.AddDays(-daysSinceMonday);

    DateTime firstWeekStart =
      currentWeekStart.AddDays(-7 * 7);

    List<OverviewWeeklyEngagementViewModel> results = new();

    for (int i = 0; i < 8; i++)
    {
      DateTime weekStart = firstWeekStart.AddDays(i * 7);
      DateTime weekEnd = weekStart.AddDays(7);

      var weeklySessions = sessions
        .Where(session => !session.IsDeleted)
        .Where(session => session.RecordingStatus == "Succeeded")
        .Where(session =>
        {
          DateTime easternDate =
            TimeZoneInfo.ConvertTime(
              session.StartedAt,
              easternTimeZone).Date;

          return easternDate >= weekStart && easternDate < weekEnd;
        })
        .ToList();

      results.Add(new OverviewWeeklyEngagementViewModel
      {
        WeekStart = new DateTimeOffset(
          weekStart,
          easternTimeZone.GetUtcOffset(weekStart)),

        ReflectXRMinutes = weeklySessions
          .Where(session => session.Platform == "Reflect XR")
          .Sum(session => session.DurationMinutes),

        ReflectMobileMinutes = weeklySessions
          .Where(session => session.Platform == "Reflect Mobile")
          .Sum(session => session.DurationMinutes)
      });
    }

    return results;
  }
}
