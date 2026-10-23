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
        ReviewStatus = "Active"
    },
    new Patient
    {
        Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
        PatientCode = "RX-7C21D904",
        ReviewStatus = "Needs Review"
    },
    new Patient
    {
        Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
        PatientCode = "RX-A8310F6E",
        ReviewStatus = "Active"
    }

  };

  public Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    IReadOnlyList<Patient> results = _patients.Where(patient => !patient.IsDeleted).ToList();
  
    return Task.FromResult(results);
  }

}