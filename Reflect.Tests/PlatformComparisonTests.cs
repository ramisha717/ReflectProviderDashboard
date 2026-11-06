using Reflect.Domain;
using Reflect.Web.Models.ViewModels.Progress.PlatformComparison;
using Reflect.Web.Repositories;
using Xunit;

namespace Reflect.Tests;

public sealed class PlatformComparisonTests
{
    private static readonly Guid DemoClinicianId = Guid.Parse("20000000-0000-0000-0000-000000000001");

    [Fact]
    public void Catalog_ContainsExactModuleCountsAndCategories()
    {
        Assert.Equal(14, ReflectProductCatalog.MobileModules.Count);
        Assert.Equal(4, ReflectProductCatalog.MobileGroups.Count);

        Assert.Equal(17, ReflectProductCatalog.XRModules.Count);
        Assert.Equal(5, ReflectProductCatalog.XRPrograms.Count);

        // Verify module group mapping
        Assert.Equal("Creative Calm", ReflectProductCatalog.GetGroupOrProgram("Reflect Mobile", "Floating Bubbles"));
        Assert.Equal("Create", ReflectProductCatalog.GetGroupOrProgram("Reflect Mobile", "Create Your Own Prompt (AI)"));
        Assert.Equal("Learn", ReflectProductCatalog.GetGroupOrProgram("Reflect Mobile", "Creativity (AI)"));
        Assert.Equal("Reflect", ReflectProductCatalog.GetGroupOrProgram("Reflect Mobile", "Reflection Journals (AI)"));

        // Verify XR program mapping
        Assert.Equal("Creative Calm", ReflectProductCatalog.GetGroupOrProgram("Reflect XR", "Containment"));
        Assert.Equal("Explore Emotions", ReflectProductCatalog.GetGroupOrProgram("Reflect XR", "Emotional Expression"));
        Assert.Equal("Explore Self", ReflectProductCatalog.GetGroupOrProgram("Reflect XR", "Future Self"));
        Assert.Equal("Explore Journey", ReflectProductCatalog.GetGroupOrProgram("Reflect XR", "Hope Tree"));
        Assert.Equal("Getting Started", ReflectProductCatalog.GetGroupOrProgram("Reflect XR", "Training Tutorial"));
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    public async Task PrepareData_PopulatesModuleMetricsAndHourlyDistribution(int days)
    {
        var sessionRepo = new InMemoryPatientSessionRepository();
        var patientRepo = new InMemoryPatientRepository();
        var accessRepo = new InMemoryPatientAccessRepository();

        var vm = new ProgressPlatformComparisonViewModel();
        await vm.PrepareData(sessionRepo, patientRepo, accessRepo, DemoClinicianId, days, default);

        // Top level metrics should be populated
        Assert.True(vm.ReflectXRSessions > 0);
        Assert.True(vm.ReflectMobileSessions > 0);
        Assert.True(vm.XRTotalDurationMinutes > 0);
        Assert.True(vm.MobileTotalDurationMinutes > 0);

        // Modules should have all 17 XR and 14 Mobile entries
        Assert.Equal(17, vm.XRModules.Count);
        Assert.Equal(14, vm.MobileModules.Count);

        // All module items should have valid non-null strings
        foreach (var m in vm.XRModules)
        {
            Assert.False(string.IsNullOrWhiteSpace(m.ModuleName));
            Assert.False(string.IsNullOrWhiteSpace(m.ProgramOrGroup));
            Assert.Equal("Reflect XR", m.Platform);
        }

        foreach (var m in vm.MobileModules)
        {
            Assert.False(string.IsNullOrWhiteSpace(m.ModuleName));
            Assert.False(string.IsNullOrWhiteSpace(m.ProgramOrGroup));
            Assert.Equal("Reflect Mobile", m.Platform);
        }

        // Hourly distribution should have 24 hours
        Assert.Equal(24, vm.XRHourlyDistribution.Count);
        Assert.Equal(24, vm.MobileHourlyDistribution.Count);

        for (int i = 0; i < 24; i++)
        {
            Assert.Equal(i, vm.XRHourlyDistribution[i].Hour);
            Assert.False(string.IsNullOrWhiteSpace(vm.XRHourlyDistribution[i].DisplayLabel));

            Assert.Equal(i, vm.MobileHourlyDistribution[i].Hour);
            Assert.False(string.IsNullOrWhiteSpace(vm.MobileHourlyDistribution[i].DisplayLabel));
        }
    }
}
