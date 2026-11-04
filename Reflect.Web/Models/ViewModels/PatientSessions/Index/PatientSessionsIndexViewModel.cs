
using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;

namespace Reflect.Web.Models.ViewModels.PatientSessions.Index;

public class PatientSessionsIndexViewModel
{
  public string SelectedPatientCode { get; set; } = string.Empty;

  public string SelectedPlatform { get; set; } = string.Empty;

  public DateOnly? StartDate { get; set; }

  public DateOnly? EndDate { get; set; }

  public string SelectedModule { get; set; } = string.Empty;

  public List<PatientSessionHistoryRowViewModel> Sessions { get; set; } = new();

  public async Task PrepareData(
    IPatientRepository patientsRepo,
    IPatientSessionRepository sessionsRepo,
    IPatientAccessRepository patientAccessRepo,
    Guid clinicianId,
    string patientCode,
    string platform,
    DateOnly? startDate,
    DateOnly? endDate,
    string module,
    CancellationToken cancellationToken)
  {
    if (startDate.HasValue && endDate.HasValue &&
        startDate.Value > endDate.Value)
    {
        throw new ArgumentException(
            "Start date cannot be after end date.");
    }

    SelectedPatientCode = patientCode ?? string.Empty;
    SelectedPlatform = platform ?? string.Empty;
    StartDate = startDate;
    EndDate = endDate;
    SelectedModule = module ?? string.Empty;

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

    var patients = authorizedPatients;

    var authorizedPatientIds = patients
        .Select(p => p.Id)
        .ToHashSet();

    var allSessions = await sessionsRepo.GetAllAsync(cancellationToken);

    var sessions = allSessions
        .Where(s => authorizedPatientIds.Contains(s.PatientId))
        .ToList();

    var patientsById = patients.ToDictionary(p => p.Id);

    var filteredSessions = sessions.AsEnumerable();

    if (!string.IsNullOrWhiteSpace(SelectedPatientCode))
    {
      var matchingPatientIds = patients
        .Where(p => p.PatientCode == SelectedPatientCode)
        .Select(p => p.Id)
        .ToHashSet();

      filteredSessions = filteredSessions.Where(
        s => matchingPatientIds.Contains(s.PatientId));
    }

    if (!string.IsNullOrWhiteSpace(SelectedPlatform))
    {
      filteredSessions = filteredSessions.Where(
        s => s.Platform == SelectedPlatform);
    }

    if (startDate.HasValue)
    {
      filteredSessions = filteredSessions.Where(
        s => DateOnly.FromDateTime(s.StartedAt.UtcDateTime)
          >= startDate.Value);
    }

    if (endDate.HasValue)
    {
      filteredSessions = filteredSessions.Where(
        s => DateOnly.FromDateTime(s.StartedAt.UtcDateTime)
          <= endDate.Value);
    }

    if (!string.IsNullOrWhiteSpace(SelectedModule))
    {
        filteredSessions = filteredSessions.Where(
            s => s.ModuleName == SelectedModule);
    }

    Sessions = filteredSessions
      .Where(s => patientsById.ContainsKey(s.PatientId))
      .OrderByDescending(s => s.StartedAt)
      .Select(s => PatientSessionHistoryRowViewModel.FromEntity(
          patientsById[s.PatientId],
          s))
      .ToList();
  }
}
