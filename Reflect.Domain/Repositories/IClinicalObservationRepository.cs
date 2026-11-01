using Reflect.Domain.Entities;

namespace Reflect.Domain.Repositories;

public interface IClinicalObservationRepository
{
    Task<List<ClinicalObservation>> GetAllAsync(CancellationToken cancellationToken);

    Task<List<ClinicalObservation>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken);

    Task AddAsync(ClinicalObservation observation, CancellationToken cancellationToken);
}