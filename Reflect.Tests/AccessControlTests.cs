using Microsoft.AspNetCore.Mvc;
using Reflect.Domain.Entities;
using Reflect.Domain.Repositories;
using Reflect.Domain.Services;
using Reflect.Web.Areas.Provider.Controllers;
using Reflect.Web.Models.ViewModels.Reports;
using Reflect.Web.Repositories;
using Reflect.Web.Services;
using Xunit;

namespace Reflect.Tests;

public sealed class AccessControlTests
{
    private static readonly Guid DemoClinician = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid AllowedPatient = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid OtherPatient = Guid.NewGuid();

    [Fact]
    public async Task DemoAclAllowsAssignedPatientAndRejectsOtherPatientsAndClinicians()
    {
        var acl = new InMemoryPatientAccessRepository();
        Assert.True(await acl.CanAccessPatientAsync(DemoClinician, AllowedPatient, default));
        Assert.False(await acl.CanAccessPatientAsync(DemoClinician, OtherPatient, default));
        Assert.False(await acl.CanAccessPatientAsync(Guid.NewGuid(), AllowedPatient, default));
    }

    [Fact]
    public async Task GrantAccessIsScopedToClinicianAndPatient()
    {
        var acl = new InMemoryPatientAccessRepository();
        var clinician = Guid.NewGuid();
        await acl.GrantAccessAsync(clinician, OtherPatient, default);
        Assert.True(await acl.CanAccessPatientAsync(clinician, OtherPatient, default));
        Assert.False(await acl.CanAccessPatientAsync(DemoClinician, OtherPatient, default));
        Assert.False(await acl.CanAccessPatientAsync(clinician, AllowedPatient, default));
    }

    [Fact]
    public async Task AuthorizationServiceDeniesMissingClinicianIdentity()
    {
        var acl = new InMemoryPatientAccessRepository();
        var service = new PatientAuthorizationService(new FakeUser(null), acl);
        Assert.False(await service.CanAccessPatientAsync(AllowedPatient, default));
    }

    [Fact]
    public async Task AuthorizationServiceUsesActualClinicianId()
    {
        var acl = new InMemoryPatientAccessRepository();
        var service = new PatientAuthorizationService(new FakeUser(DemoClinician), acl);
        Assert.True(await service.CanAccessPatientAsync(AllowedPatient, default));
        Assert.False(await service.CanAccessPatientAsync(OtherPatient, default));
    }

    [Fact]
    public async Task PatientSessionsControllerDeniesUnassignedPatientBeforeReadingSessions()
    {
        var repository = new RecordingSessions();
        var controller = new PatientSessionsController(repository, new FixedAuthorization(false));
        var response = await controller.Index(default, OtherPatient);
        Assert.IsType<ForbidResult>(response);
        Assert.False(repository.WasCalled);
    }

    [Fact]
    public async Task ReportsPreviewDeniesUnassignedPatient()
    {
        var controller = new ReportsController(null, null, null, null, null,
            new FixedAuthorization(false));
        var result = await controller.Preview(new ReportRequest { PatientId = OtherPatient }, default);
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task ReportsRecentExportsDeniesUnassignedPatient()
    {
        var controller = new ReportsController(null, null, null, null, null,
            new FixedAuthorization(false));
        var result = await controller.RecentExports(OtherPatient, default);
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task ReportsExportDeniesUnassignedPatient()
    {
        var controller = new ReportsController(null, null, null, null,
            new FakeUser(DemoClinician), new FixedAuthorization(false));
        var result = await controller.Export(new ReportRequest { PatientId = OtherPatient },
            "csv", default);
        Assert.IsType<ForbidResult>(result);
    }

    private sealed class FixedAuthorization(bool allowed) : IPatientAuthorizationService
    {
        public Task<bool> CanAccessPatientAsync(Guid patientId, CancellationToken cancellationToken)
            => Task.FromResult(allowed);
    }

    private sealed class RecordingSessions : IPatientSessionRepository
    {
        public bool WasCalled { get; private set; }
        public Task<IReadOnlyList<PatientSession>> GetSessionsByPatientAsync(
            Guid patientId, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult<IReadOnlyList<PatientSession>>(Array.Empty<PatientSession>());
        }
        public Task<IReadOnlyList<PatientSession>> GetAllAsync(CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult<IReadOnlyList<PatientSession>>(Array.Empty<PatientSession>());
        }
    }

    private sealed class FakeUser(Guid? clinicianId) : ICurrentUserService
    {
        public Guid? ClinicianId => clinicianId;
        public Guid? UserId => clinicianId;
        public string EntraObjectId => "";
        public string Email => "";
        public bool IsAuthenticated => clinicianId.HasValue;
        public bool IsInRole(string role) => role == "Clinician" && clinicianId.HasValue;
    }
}
