
using Reflect.Domain.Repositories;

namespace Reflect.Web.Models.ViewModels.Progress.Index;

public class ProgressIndexViewModel
{
    public int SelectedDays { get; set; } = 30;

    public int TotalSessions { get; set; }

    public int ActiveDays { get; set; }

    public double MedianDurationMinutes { get; set; }

    public List<ProgressWeeklyActivityViewModel> WeeklyActivity { get; set; } = new();

    public List<ProgressDurationBucketViewModel> DurationBuckets { get; set; } = new();

    public async Task PrepareData(
      IPatientSessionRepository sessionsRepo,
      IPatientRepository patientsRepo,
      IPatientAccessRepository patientAccessRepo,
      Guid clinicianId,
      int days,
      CancellationToken cancellationToken)
    {
      if (days != 30 && days != 60)
      {
          throw new ArgumentOutOfRangeException(
              nameof(days),
              "Only 30 or 60 days are supported.");
      }

      SelectedDays = days;

      // The UI labels are approximate:
      // 30 days = 4 weeks; 60 days = 8 weeks.
      int actualDays = days == 30 ? 28 : 56;

      DateTimeOffset now = DateTimeOffset.UtcNow;
      DateTimeOffset start = now.AddDays(-actualDays);

      var patients = await patientsRepo.GetAllAsync(cancellationToken);

      var authorizedPatientIds = new HashSet<Guid>();

      foreach (var patient in patients)
      {
          if (await patientAccessRepo.CanAccessPatientAsync(
              clinicianId,
              patient.Id,
              cancellationToken))
          {
              authorizedPatientIds.Add(patient.Id);
          }
      }

      var allSessions = await sessionsRepo.GetAllAsync(cancellationToken);

      var sessions = allSessions
          .Where(s =>
              authorizedPatientIds.Contains(s.PatientId) &&
              s.StartedAt >= start &&
              s.StartedAt < now)
          .ToList();

      TotalSessions = sessions.Count;

      ActiveDays = sessions
          .Select(s => s.StartedAt.UtcDateTime.Date)
          .Distinct()
          .Count();

      var durations = sessions
          .Select(s => s.DurationMinutes)
          .OrderBy(duration => duration)
          .ToList();

      if (durations.Count == 0)
      {
          MedianDurationMinutes = 0;
      }
      else if (durations.Count % 2 == 1)
      {
          MedianDurationMinutes = durations[durations.Count / 2];
      }
      else
      {
          int middle = durations.Count / 2;

          MedianDurationMinutes =
              (durations[middle - 1] + durations[middle]) / 2.0;
      }

      WeeklyActivity = ProgressWeeklyActivityViewModel.FromSessions(
          sessions, start, actualDays / 7);

      DurationBuckets = ProgressDurationBucketViewModel.FromSessions(sessions);
    }
}
