using Reflect.Domain.Entities;

namespace Reflect.Web.Models.ViewModels.Patients.Index;

public class PatientRowViewModel 
{
  public Guid PatientId { get; set; }
  public string PatientCode { get; set; } = string.Empty;
  public DateTimeOffset? LastActive { get; set; }
  public int NumOfSessions { get; set; }
  public string Platform { get; set; } = string.Empty;
  public string Trend { get; set; } = string.Empty;
  public string ReviewStatus { get; set; } = string.Empty;

  public static PatientRowViewModel FromEntity(Patient patient, IReadOnlyList<PatientSession> sessions)
  {

    var successfulSessions = sessions
    .Where(session => session.RecordingStatus == "Succeeded")
    .OrderByDescending(session => session.StartedAt).ToList();

    var lastSession = successfulSessions.FirstOrDefault();

    

    return new PatientRowViewModel
    {
    PatientId = patient.Id,
    PatientCode = patient.PatientCode, 

    LastActive = lastSession?.StartedAt,
    NumOfSessions = successfulSessions.Count,
    Platform = lastSession?.Platform ?? string.Empty,
    Trend = string.Empty,
    ReviewStatus = patient.ReviewStatus
    };

  }

}



