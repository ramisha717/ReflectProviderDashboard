
using Reflect.Domain.Entities;
using Reflect.Domain.Enums;
using Reflect.Domain.Services;
using Reflect.Web.Extensions;

namespace Reflect.Web.Services;

public class ReportGenerationService : IReportGenerationService
{
  public ReportContent GenerateReport(
    ReportType reportType,
    Patient patient,
    IReadOnlyList<PatientSession> sessions,
    DateTimeOffset periodStart,
    DateTimeOffset periodEnd,
    string platformFilter,
    bool includeSessionCountAndDuration,
    bool includePlatformActivity,
    bool includeModuleActivity,
    bool includeSessionDetails)
  {
    ArgumentNullException.ThrowIfNull(patient);
    ArgumentNullException.ThrowIfNull(sessions);

    if (periodStart > periodEnd)
        throw new ArgumentException(
            "The reporting period start must not be after its end.");

    if (!Enum.IsDefined(reportType) ||
        reportType == ReportType.RtmStatus)
    {
      throw new NotSupportedException(
        "The selected report template is not currently supported.");
    }

    if (!string.IsNullOrWhiteSpace(platformFilter) &&
      platformFilter != "Reflect XR" &&
      platformFilter != "Reflect Mobile")
    {
      throw new ArgumentException(
        "Platform must be Reflect XR, Reflect Mobile, or unspecified.");
    }

    var filteredSessions = sessions
      .Where(s =>
        s.PatientId == patient.Id &&
        s.RecordingStatus == "Succeeded" &&
        s.StartedAt >= periodStart &&
        s.StartedAt <= periodEnd &&
        (string.IsNullOrWhiteSpace(platformFilter) || s.Platform == platformFilter))
      .OrderBy(s => s.StartedAt)
      .ToList();

    var report = new ReportContent
    {
      ReportType = reportType,
      PatientCode = patient.PatientCode,
      PeriodStart = periodStart,
      PeriodEnd = periodEnd,
      PlatformFilter = platformFilter,
      Title = reportType switch
      {
          ReportType.EngagementTrends => "Engagement Trends",
          ReportType.PlatformComparison => "XR vs Mobile",
          ReportType.SessionLog => "Session Log",
          _ => throw new NotSupportedException()
      }
    };

    switch (reportType)
    {
      case ReportType.EngagementTrends:
        AddEngagementTrends(
          report,
          filteredSessions,
          includeSessionCountAndDuration,
          includePlatformActivity,
          includeModuleActivity,
          includeSessionDetails);
        break;

      case ReportType.PlatformComparison:
        AddPlatformComparison(
          report,
          filteredSessions,
          includeSessionCountAndDuration,
          includePlatformActivity,
          includeModuleActivity,
          includeSessionDetails);
        break;

      case ReportType.SessionLog:
        AddSessionLog(
          report,
          filteredSessions,
          includeSessionCountAndDuration,
          includePlatformActivity,
          includeModuleActivity,
          includeSessionDetails);
          break;
    }

    return report;
  }

  private static void AddEngagementTrends(
      ReportContent report,
      List<PatientSession> sessions,
      bool includeSessionCountAndDuration,
      bool includePlatformActivity,
      bool includeModuleActivity,
      bool includeSessionDetails)
  {
    if (includeSessionCountAndDuration)
    {
      var section = new ReportSection
      {
          Heading = "Engagement Overview"
      };

      section.Lines.Add(
          $"Total sessions: {sessions.Count}");

      section.Lines.Add(
          $"Total engagement minutes: {sessions.Sum(s => s.DurationMinutes)}");

      section.Lines.Add(
          $"Median session duration: {CalculateMedian(sessions)} minutes");

      report.Sections.Add(section);

      var weeklySection = new ReportSection
      {
          Heading = "Weekly Engagement"
      };

      var weeklyGroups = sessions
          .GroupBy(s => System.Globalization.ISOWeek
              .GetYear(s.StartedAt.UtcDateTime).ToString() +
              "-W" +
              System.Globalization.ISOWeek
                  .GetWeekOfYear(s.StartedAt.UtcDateTime)
                  .ToString("D2"))
          .OrderBy(g => g.Key);

      foreach (var group in weeklyGroups)
      {
          weeklySection.Lines.Add(
              $"{group.Key}: {group.Count()} sessions, " +
              $"{group.Sum(s => s.DurationMinutes)} minutes");
      }

      if (weeklySection.Lines.Count == 0)
          weeklySection.Lines.Add("No sessions recorded.");

      report.Sections.Add(weeklySection);
  }

    AddPlatformSection(report, sessions, includePlatformActivity);
    AddModuleSection(report, sessions, includeModuleActivity);
    AddSessionDetailsSection(report, sessions, includeSessionDetails);
  }

  private static void AddPlatformComparison(
      ReportContent report,
      List<PatientSession> sessions,
      bool includeSessionCountAndDuration,
      bool includePlatformActivity,
      bool includeModuleActivity,
      bool includeSessionDetails)
  {
      if (includePlatformActivity)
      {
          var section = new ReportSection
          {
              Heading = "Platform Comparison"
          };

          foreach (var platform in new[] { "Reflect XR", "Reflect Mobile" })
          {
              var platformSessions = sessions
                  .Where(s => s.Platform == platform)
                  .ToList();

              section.Lines.Add(
                  $"{platform}: {platformSessions.Count} sessions");

              section.Lines.Add(
                  $"{platform} total duration: " +
                  $"{platformSessions.Sum(s => s.DurationMinutes)} minutes");

              section.Lines.Add(
                  $"{platform} median duration: " +
                  $"{CalculateMedian(platformSessions)} minutes");
          }

          report.Sections.Add(section);
      }

      AddCountAndDurationSection(
          report, sessions, includeSessionCountAndDuration);

      AddModuleSection(report, sessions, includeModuleActivity);
      AddSessionDetailsSection(report, sessions, includeSessionDetails);
  }

  private static void AddSessionLog(
      ReportContent report,
      List<PatientSession> sessions,
      bool includeSessionCountAndDuration,
      bool includePlatformActivity,
      bool includeModuleActivity,
      bool includeSessionDetails)
  {
      AddSessionDetailsSection(report, sessions, includeSessionDetails);

      AddCountAndDurationSection(
          report, sessions, includeSessionCountAndDuration);

      AddPlatformSection(report, sessions, includePlatformActivity);
      AddModuleSection(report, sessions, includeModuleActivity);
  }

  private static void AddCountAndDurationSection(
      ReportContent report,
      List<PatientSession> sessions,
      bool include)
  {
      if (!include)
          return;

      report.Sections.Add(new ReportSection
      {
          Heading = "Session Summary",
          Lines = new List<string>
          {
              $"Total sessions: {sessions.Count}",
              $"Total duration: {sessions.Sum(s => s.DurationMinutes)} minutes",
              $"Median duration: {CalculateMedian(sessions)} minutes"
          }
      });
  }

  private static void AddPlatformSection(
      ReportContent report,
      List<PatientSession> sessions,
      bool include)
  {
      if (!include)
          return;

      var section = new ReportSection
      {
          Heading = "Platform Activity"
      };

      foreach (var group in sessions
          .GroupBy(s => s.Platform)
          .OrderBy(g => g.Key))
      {
          section.Lines.Add(
              $"{group.Key}: {group.Count()} sessions");
      }

      if (section.Lines.Count == 0)
          section.Lines.Add("No platform activity recorded.");

      report.Sections.Add(section);
  }

  private static void AddModuleSection(
      ReportContent report,
      List<PatientSession> sessions,
      bool include)
  {
      if (!include)
          return;

      var section = new ReportSection
      {
          Heading = "Module Activity"
      };

      foreach (var group in sessions
          .GroupBy(s => s.ModuleName)
          .OrderBy(g => g.Key))
      {
          section.Lines.Add(
              $"{group.Key}: {group.Count()} sessions");
      }

      if (section.Lines.Count == 0)
          section.Lines.Add("No module activity recorded.");

      report.Sections.Add(section);
  }

  private static void AddSessionDetailsSection(
      ReportContent report,
      List<PatientSession> sessions,
      bool include)
  {
      if (!include)
          return;

      var section = new ReportSection
      {
          Heading = "Session Details"
      };

      foreach (var session in sessions)
      {
          section.Lines.Add(
              $"{session.StartedAt.ToUsDateTimeString()} | " +
              $"{session.Platform} | " +
              $"{session.ModuleName} | " +
              $"{session.DurationMinutes} minutes");
      }

      if (section.Lines.Count == 0)
          section.Lines.Add("No sessions recorded.");

      report.Sections.Add(section);
  }

  private static double CalculateMedian(
      List<PatientSession> sessions)
  {
      if (sessions.Count == 0)
          return 0;

      var durations = sessions
          .Select(s => s.DurationMinutes)
          .OrderBy(d => d)
          .ToArray();

      int middle = durations.Length / 2;

      if (durations.Length % 2 == 1)
          return durations[middle];

      return ((double)durations[middle - 1] + durations[middle]) / 2;
  }
}
