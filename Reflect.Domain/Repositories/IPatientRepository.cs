using Reflect.Domain.Entities;

namespace Reflect.Domain.Repositories;

public interface IPatientRepository
{
  Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken);

  Task<Patient?> GetByIdAsync(
    Guid patientId,
    CancellationToken cancellationToken);

  Task SetReviewStatusAsync(
    Guid patientId,
    string status,
    CancellationToken cancellationToken);

}