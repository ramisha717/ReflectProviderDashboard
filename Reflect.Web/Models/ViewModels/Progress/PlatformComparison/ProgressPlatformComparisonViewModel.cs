
using Reflect.Domain.Repositories;
using Reflect.Web.Models.ViewModels.Progress.Index;

namespace Reflect.Web.Models.ViewModels.Progress.PlatformComparison;

public class ProgressPlatformComparisonViewModel
{
  public int SelectedDays { get; set; } = 30;

  public int ReflectXRSessions { get; set; }

  public int ReflectMobileSessions { get; set; }

  public double XRMedianDurationMinutes { get; set; }

  public double MobileMedianDurationMinutes { get; set; }

  public int XRTotalDurationMinutes { get; set; }

  public int MobileTotalDurationMinutes { get; set; }

  public int XRActiveDays { get; set; }

  public int MobileActiveDays { get; set; }

  public List<ProgressWeeklyActivityViewModel> WeeklyActivity { get; set; } = new();

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

    var xrSessions = sessions
      .Where(s => s.Platform == "Reflect XR")
      .ToList();

    var mobileSessions = sessions
      .Where(s => s.Platform == "Reflect Mobile")
      .ToList();

    ReflectXRSessions = xrSessions.Count;
    ReflectMobileSessions = mobileSessions.Count;

    XRMedianDurationMinutes = CalculateMedian(xrSessions.Select(s => s.DurationMinutes));

    MobileMedianDurationMinutes = CalculateMedian(mobileSessions.Select(s => s.DurationMinutes));

    XRTotalDurationMinutes = xrSessions.Sum(s => s.DurationMinutes);

    MobileTotalDurationMinutes = mobileSessions.Sum(s => s.DurationMinutes);

    XRActiveDays = xrSessions
      .Select(s => s.StartedAt.UtcDateTime.Date)
      .Distinct()
      .Count();

    MobileActiveDays = mobileSessions
      .Select(s => s.StartedAt.UtcDateTime.Date)
      .Distinct()
      .Count();

    WeeklyActivity = ProgressWeeklyActivityViewModel.FromSessions(
        sessions, start, actualDays / 7);
  }

  private static double CalculateMedian(IEnumerable<int> values)
  {
    var sorted = values.OrderBy(x => x).ToList();

    if (sorted.Count == 0)
    {
        return 0;
    }

    int middle = sorted.Count / 2;

    if (sorted.Count % 2 == 1)
    {
        return sorted[middle];
    }

    return (sorted[middle - 1] + sorted[middle]) / 2.0;
  }
}
