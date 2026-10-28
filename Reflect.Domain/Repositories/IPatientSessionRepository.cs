using Reflect.Domain.Entities;

namespace Reflect.Domain.Repositories;

public interface IPatientSessionRepository
{
  Task<IReadOnlyList<PatientSession>> GetSessionsByPatientAsync(
    Guid patientId,
    CancellationToken cancellationToken);

  Task<IReadOnlyList<PatientSession>> GetAllAsync(
    CancellationToken cancellationToken);
}