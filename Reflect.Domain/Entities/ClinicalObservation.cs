namespace Reflect.Domain.Entities;

public class ClinicalObservation : BaseEntity
{
    public Guid PatientId { get; set; }

    public Guid? PatientSessionId { get; set; }

    public Guid ClinicianId { get; set; }

    public DateTimeOffset ObservedAt { get; set; }

    public string ObservationType { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
}