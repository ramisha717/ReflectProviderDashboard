namespace Reflect.Domain.Entities;

public class Patient : BaseEntity
{
  public string PatientCode { get; set; } = string.Empty;

  public string ReviewStatus { get; set; } = string.Empty;

  public bool IsActive { get; set; }
}