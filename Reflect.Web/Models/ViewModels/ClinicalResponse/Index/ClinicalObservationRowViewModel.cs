using Reflect.Domain.Entities;

namespace Reflect.Web.Models.ViewModels.ClinicalResponse.Index;

public class ClinicalObservationRowViewModel
{
  public Guid ObservationId { get; set; }

  public string ObservationType { get; set; } = string.Empty;

  public string Notes { get; set; } = string.Empty;

  public DateTimeOffset ObservedAt { get; set; }

  public Guid ClinicianId { get; set; }

  public static ClinicalObservationRowViewModel FromEntity(
    ClinicalObservation observation)
  {
    return new ClinicalObservationRowViewModel
    {
      ObservationId = observation.Id,
      ObservationType = observation.ObservationType,
      Notes = observation.Notes,
      ObservedAt = observation.ObservedAt,
      ClinicianId = observation.ClinicianId
    };
  }
}