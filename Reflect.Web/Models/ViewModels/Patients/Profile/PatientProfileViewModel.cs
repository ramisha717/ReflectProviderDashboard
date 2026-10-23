using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;

namespace Reflect.Web.Models.ViewModels.Patients.Profile;

public class PatientProfileViewModel
{ 
  public Guid PatientId { get; set; }
  public string PatientCode { get; set; } = string.Empty;
  public string ReviewStatus { get; set; } = string.Empty;
  public int NumOfSessions { get; set; }
  public int TotalDurationMinutes { get; set; }
  public DateTimeOffset? LastActive { get; set; }
  public string MostUsedPlatform { get; set; } = string.Empty;
  public int ReflectXRSessionCount { get; set; }
  public int ReflectMobileSessionCount { get; set; }
  public string MostUsedModule { get; set; } = string.Empty;
  public List<PatientProfileSessionRowViewModel> Sessions { get; set; } = new();
  public List<PatientProfileWeeklyEngagementViewModel> WeeklyEngagement { get; set; } = new();

  public async Task PrepareData(
    Guid patientId,
    IPatientRepository patientsRepo, 
    IPatientSessionRepository sessionsRepo, 
    CancellationToken cancellationToken)
  {
    var patient = await patientsRepo.GetByIdAsync(patientId, cancellationToken);

    if (patient == null)
    {
      throw new EntityNotFoundException("Patient not found.");
    }

    var sessions = await sessionsRepo.GetSessionsByPatientAsync(patientId, cancellationToken);

    var successfulSessions = sessions
      .Where(session => session.RecordingStatus == "Succeeded")
      .OrderByDescending(session => session.StartedAt)
      .ToList();

    var lastSession = successfulSessions.FirstOrDefault();
    Sessions = sessions
    .OrderByDescending(session => session.StartedAt)
    .Select(session => PatientProfileSessionRowViewModel.FromEntity(session))
    .ToList();

    
  var easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

  var latestWeekStart = DateTime.Today;

  var latestEasternDate = TimeZoneInfo.ConvertTime(
    DateTimeOffset.UtcNow,
    easternTimeZone).Date;

  latestWeekStart = latestEasternDate.AddDays(-((int)latestEasternDate.DayOfWeek + 6) % 7);

  var firstWeekStart = latestWeekStart.AddDays(-7 * 7);

  var weeklySessions = successfulSessions
    .GroupBy(session =>
    {
      var easternDate = TimeZoneInfo.ConvertTime(
        session.StartedAt,
        easternTimeZone).Date;

      return easternDate.AddDays(-((int)easternDate.DayOfWeek + 6) % 7);
    })
    .ToDictionary(group => group.Key, group => group.ToList());

  WeeklyEngagement = Enumerable.Range(0, 8)
    .Select(index =>
    {
      var weekStart = firstWeekStart.AddDays(index * 7);

      weeklySessions.TryGetValue(weekStart, out var weekSessions);

      var sessionsForWeek = weekSessions
        ?? new List<PatientSession>();

      return new PatientProfileWeeklyEngagementViewModel
      {
        WeekStart = new DateTimeOffset(
          weekStart,
          easternTimeZone.GetUtcOffset(weekStart)),

        ReflectXRMinutes = sessionsForWeek
          .Where(session => session.Platform == "Reflect XR")
          .Sum(session => session.DurationMinutes),

        ReflectMobileMinutes = sessionsForWeek
          .Where(session => session.Platform == "Reflect Mobile")
          .Sum(session => session.DurationMinutes)
      };
    })
    .ToList();




    PatientId = patient.Id;
    PatientCode = patient.PatientCode;
    ReviewStatus = patient.ReviewStatus;

    NumOfSessions = successfulSessions.Count;

    TotalDurationMinutes = successfulSessions
      .Sum(session => session.DurationMinutes);

    LastActive = lastSession?.StartedAt;

    ReflectXRSessionCount = successfulSessions
      .Count(session => session.Platform == "Reflect XR");

    ReflectMobileSessionCount = successfulSessions
      .Count(session => session.Platform == "Reflect Mobile");

    MostUsedPlatform = successfulSessions
      .GroupBy(session => session.Platform)
      .OrderByDescending(group => group.Count())
      .Select(group => group.Key)
      .FirstOrDefault() ?? string.Empty;

    MostUsedModule = successfulSessions
      .GroupBy(session => session.ModuleName)
      .OrderByDescending(group => group.Count())
      .Select(group => group.Key)
      .FirstOrDefault() ?? string.Empty;
  }

}
