using Reflect.Domain.Repositories;
using Reflect.Domain.Entities;

namespace Reflect.Web.Repositories;

public class InMemoryPatientRepository : IPatientRepository
{

  private readonly List<Patient> _patients = new()
  {
    new Patient
    {
      Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
      PatientCode = "RX-00F3A7B2",
      IsActive = true,
      ReviewStatus = "Reviewed"
    },
    new Patient
    {
      Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
      PatientCode = "RX-7C21D904",
      IsActive = true,
      ReviewStatus = "Needs Review"
    },
    new Patient
    {
      Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
      PatientCode = "RX-A8310F6E",
      IsActive = false,
      ReviewStatus = "Reviewed"
    },
    new Patient
    {
      Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
      PatientCode = "RX-4D92B105",
      IsActive = true,
      ReviewStatus = "Reviewed"
    }
  };


  public Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    IReadOnlyList<Patient> results = _patients.Where(patient => !patient.IsDeleted).ToList();
  
    return Task.FromResult(results);
  }

  public Task<Patient> GetByIdAsync(
    Guid patientId,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    Patient result = _patients
      .Where(patient => !patient.IsDeleted)
      .FirstOrDefault(patient => patient.Id == patientId);

    return Task.FromResult(result);
  }


  public Task SetReviewStatusAsync(
    Guid patientId,
    string status,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    lock (_patients)
    {
      Patient patient = _patients.FirstOrDefault(
        p => p.Id == patientId && !p.IsDeleted);

      if (patient is null)
      {
        throw new EntityNotFoundException("Patient not found.");
      }

      patient.ReviewStatus = status;
    }

    return Task.CompletedTask;
  }

  public Task AddAsync(
  Patient patient,
  CancellationToken cancellationToken)
{
  cancellationToken.ThrowIfCancellationRequested();

  lock (_patients)
  {
    if (_patients.Any(p => p.Id == patient.Id ||
      p.PatientCode == patient.PatientCode))
    {
        throw new InvalidOperationException("A patient with this ID or code already exists.");
    }

      _patients.Add(patient);
  }

  return Task.CompletedTask;
}


}