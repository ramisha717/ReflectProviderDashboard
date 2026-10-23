using Reflect.Domain.Entities;

namespace Reflect.Domain.Repositories;

public interface IPatientRepository
{
  Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken);
}