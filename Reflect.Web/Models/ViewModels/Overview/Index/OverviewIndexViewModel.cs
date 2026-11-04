using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;
using Reflect.Web.Services;

namespace Reflect.Web.Models.ViewModels.Overview.Index;

public class OverviewIndexViewModel
{
  public int TotalPatients { get; set; }
  public int ActivePatients { get; set; }
  public int SessionsThisMonth { get; set; }
  public int PatientsNeedingReview { get; set; }

  public int? InactivityThresholdDays { get; set; }
  public int? FrequencyDeclineThresholdPercent { get; set; }

  public List<OverviewRecentActivityRowViewModel> RecentActivity { get; set; } = new();
  public List<OverviewWeeklyEngagementViewModel> WeeklyEngagement { get; set; } = new();
  public List<OverviewAttentionRowViewModel> AttentionQueue { get; set; } = new();

  public async Task PrepareData(
    IPatientRepository patientsRepo,
    IPatientSessionRepository sessionsRepo,
    IPatientAccessRepository patientAccessRepo,
    AttentionAcknowledgementService acknowledgements,
    Guid clinicianId,
    int? inactivityThresholdDays,
    int? frequencyDeclineThresholdPercent,
    CancellationToken cancellationToken)
  {


    if (inactivityThresholdDays.HasValue && inactivityThresholdDays.Value < 1)
    {
      throw new ArgumentOutOfRangeException(nameof(inactivityThresholdDays));
    }

    if (frequencyDeclineThresholdPercent.HasValue &&
      (frequencyDeclineThresholdPercent.Value < 1 ||
        frequencyDeclineThresholdPercent.Value > 100))
    {
      throw new ArgumentOutOfRangeException(nameof(frequencyDeclineThresholdPercent));
    }

    InactivityThresholdDays = inactivityThresholdDays;
    FrequencyDeclineThresholdPercent = frequencyDeclineThresholdPercent;


   var allPatients = await patientsRepo.GetAllAsync(cancellationToken);

var authorizedPatients = new List<Patient>();

foreach (var patient in allPatients)
{
    if (await patientAccessRepo.CanAccessPatientAsync(
        clinicianId,
        patient.Id,
        cancellationToken))
    {
        authorizedPatients.Add(patient);
    }
}

    IReadOnlyList<Patient> patients = authorizedPatients;

    var authorizedPatientIds = authorizedPatients
        .Select(patient => patient.Id)
        .ToHashSet();

    var allSessions = await sessionsRepo.GetAllAsync(cancellationToken);

    IReadOnlyList<PatientSession> sessions = allSessions
        .Where(session => authorizedPatientIds.Contains(session.PatientId))
        .ToList();

    TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

    DateTimeOffset now = DateTimeOffset.UtcNow;

    DateTimeOffset nowEastern = TimeZoneInfo.ConvertTime(now, easternTimeZone);

    TotalPatients = patients.Count;

    ActivePatients = patients.Count(patient => patient.IsActive);

    PatientsNeedingReview = patients.Count(patient => patient.ReviewStatus == "Needs Review");

    SessionsThisMonth = sessions.Count(session =>
    {
      if (session.RecordingStatus != "Succeeded")
      {
        return false;
      }

      DateTimeOffset sessionEastern = TimeZoneInfo.ConvertTime(session.StartedAt, easternTimeZone);

      return sessionEastern.Year == nowEastern.Year && sessionEastern.Month == nowEastern.Month;
    });

    Dictionary<Guid, Patient> patientsById =
        patients.ToDictionary(patient => patient.Id);

    RecentActivity = sessions
    .Where(session => patientsById.ContainsKey(session.PatientId))
    .OrderByDescending(session => session.StartedAt)
    .Take(5)
    .Select(session =>
      OverviewRecentActivityRowViewModel.FromEntity(
        patientsById[session.PatientId],
        session))
    .ToList();

    WeeklyEngagement = OverviewWeeklyEngagementViewModel.FromSessions(sessions);

    AttentionQueue = new List<OverviewAttentionRowViewModel>();

    foreach (Patient patient in patients.Where(patient => patient.IsActive))
    {
      List<PatientSession> patientSessions = sessions
        .Where(session =>
          session.PatientId == patient.Id &&
          session.RecordingStatus == "Succeeded" &&
          session.StartedAt <= now)
        .OrderByDescending(session => session.StartedAt)
        .ToList();

      // Clinical workflow: explicitly marked for review.
      if (patient.ReviewStatus == "Needs Review")
      {
        AttentionQueue.Add(new OverviewAttentionRowViewModel
        {
          PatientId = patient.Id,
          PatientCode = patient.PatientCode,
          SignalType = "Needs Review",
          Reason = "Patient record is marked as needing review."
        });
      }

      // Product analytics: inactivity.
      if (inactivityThresholdDays.HasValue &&
          patientSessions.Count > 0)
      {
        DateTimeOffset lastActivity = patientSessions[0].StartedAt;

        double inactiveDays = (now - lastActivity).TotalDays;

        if (inactiveDays >= inactivityThresholdDays.Value &&
            !acknowledgements.IsAcknowledged(clinicianId, patient.Id, "Inactivity", lastActivity))
        {
          AttentionQueue.Add(new OverviewAttentionRowViewModel
          {
            PatientId = patient.Id,
            PatientCode = patient.PatientCode,
            SignalType = "Inactivity",
            Reason =
                $"No recorded Reflect activity for " +
                $"{(int)inactiveDays} complete days."
          });
        }
      }

      // Product analytics: frequency decline.
      if (frequencyDeclineThresholdPercent.HasValue)
      {
        DateTimeOffset recentStart = now.AddDays(-30);
        DateTimeOffset previousStart = now.AddDays(-60);

        int recentCount = patientSessions.Count(session => session.StartedAt > recentStart);

        int previousCount = patientSessions.Count(session =>
          session.StartedAt > previousStart &&
          session.StartedAt <= recentStart);

        if (previousCount > 0)
        {
          double declinePercent =
              (previousCount - recentCount) * 100.0 /
              previousCount;

          if (declinePercent >=
              frequencyDeclineThresholdPercent.Value &&
              !acknowledgements.IsAcknowledged(clinicianId, patient.Id, "Frequency Decline",
                  patientSessions.Count > 0 ? patientSessions[0].StartedAt : DateTimeOffset.MinValue))
          {
            AttentionQueue.Add(new OverviewAttentionRowViewModel
            {
              PatientId = patient.Id,
              PatientCode = patient.PatientCode,
              SignalType = "Frequency Decline",
              Reason =
                  $"Recorded session frequency decreased by " +
                  $"{declinePercent:F1}% compared with the " +
                  $"preceding 30-day period."
            });
          }
      }
      }
    }
  }
}