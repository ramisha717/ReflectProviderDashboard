using Reflect.Domain;
using Reflect.Domain.Repositories;
using Reflect.Web.Models.ViewModels.Progress.Index;

namespace Reflect.Web.Models.ViewModels.Progress.PlatformComparison;

public class ProgressPlatformComparisonViewModel
{
    public int SelectedDays { get; set; } = 30;

    public int ReflectXRSessions { get; set; }

    public int ReflectMobileSessions { get; set; }

    public double XRMedianDurationMinutes { get; set; }

    public double MobileMedianDurationMinutes { get; set; }

    public int XRTotalDurationMinutes { get; set; }

    public int MobileTotalDurationMinutes { get; set; }

    public int XRActiveDays { get; set; }

    public int MobileActiveDays { get; set; }

    public List<ProgressWeeklyActivityViewModel> WeeklyActivity { get; set; } = new();

    // Module Comparisons
    public List<ModuleComparisonItemViewModel> XRModules { get; set; } = new();

    public List<ModuleComparisonItemViewModel> MobileModules { get; set; } = new();

    public int XRMaxModuleSessions { get; set; } = 1;

    public int MobileMaxModuleSessions { get; set; } = 1;

    // Hourly Breakdowns
    public List<HourlyDistributionItemViewModel> XRHourlyDistribution { get; set; } = new();

    public List<HourlyDistributionItemViewModel> MobileHourlyDistribution { get; set; } = new();

    public int XRMaxHourlyTotal { get; set; } = 1;

    public int MobileMaxHourlyTotal { get; set; } = 1;

    public IReadOnlyList<string> XRPrograms => ReflectProductCatalog.XRPrograms;

    public IReadOnlyList<string> MobileGroups => ReflectProductCatalog.MobileGroups;

    public async Task PrepareData(
        IPatientSessionRepository sessionsRepo,
        IPatientRepository patientsRepo,
        IPatientAccessRepository patientAccessRepo,
        Guid clinicianId,
        int days,
        CancellationToken cancellationToken)
    {
        if (days != 30 && days != 60)
        {
            throw new ArgumentOutOfRangeException(
                nameof(days),
                "Only 30 or 60 days are supported.");
        }

        SelectedDays = days;

        int actualDays = days == 30 ? 28 : 56;

        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset start = now.AddDays(-actualDays);

        var patients = await patientsRepo.GetAllAsync(cancellationToken);

        var authorizedPatientIds = new HashSet<Guid>();

        foreach (var patient in patients)
        {
            if (await patientAccessRepo.CanAccessPatientAsync(
                clinicianId,
                patient.Id,
                cancellationToken))
            {
                authorizedPatientIds.Add(patient.Id);
            }
        }

        var allSessions = await sessionsRepo.GetAllAsync(cancellationToken);

        var sessions = allSessions
            .Where(s =>
                authorizedPatientIds.Contains(s.PatientId) &&
                s.StartedAt >= start &&
                s.StartedAt < now)
            .ToList();

        var xrSessions = sessions
            .Where(s => s.Platform == ReflectProductCatalog.PlatformXR)
            .ToList();

        var mobileSessions = sessions
            .Where(s => s.Platform == ReflectProductCatalog.PlatformMobile)
            .ToList();

        ReflectXRSessions = xrSessions.Count;
        ReflectMobileSessions = mobileSessions.Count;

        XRMedianDurationMinutes = CalculateMedian(xrSessions.Select(s => s.DurationMinutes));
        MobileMedianDurationMinutes = CalculateMedian(mobileSessions.Select(s => s.DurationMinutes));

        XRTotalDurationMinutes = xrSessions.Sum(s => s.DurationMinutes);
        MobileTotalDurationMinutes = mobileSessions.Sum(s => s.DurationMinutes);

        XRActiveDays = xrSessions
            .Select(s => s.StartedAt.UtcDateTime.Date)
            .Distinct()
            .Count();

        MobileActiveDays = mobileSessions
            .Select(s => s.StartedAt.UtcDateTime.Date)
            .Distinct()
            .Count();

        WeeklyActivity = ProgressWeeklyActivityViewModel.FromSessions(
            sessions, start, actualDays / 7);

        // Prepare XR Module Comparison (all 17 modules)
        XRModules = new List<ModuleComparisonItemViewModel>();
        foreach (var (moduleName, program) in ReflectProductCatalog.XRModules)
        {
            var matchingSessions = xrSessions
                .Where(s => string.Equals(s.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var count = matchingSessions.Count;
            var duration = matchingSessions.Sum(s => s.DurationMinutes);
            var pct = ReflectXRSessions > 0 ? (count * 100.0 / ReflectXRSessions) : 0.0;

            XRModules.Add(new ModuleComparisonItemViewModel
            {
                ModuleName = moduleName,
                ProgramOrGroup = program,
                Platform = ReflectProductCatalog.PlatformXR,
                SessionCount = count,
                TotalDurationMinutes = duration,
                PercentageOfPlatform = pct
            });
        }
        XRMaxModuleSessions = Math.Max(1, XRModules.Select(m => m.SessionCount).DefaultIfEmpty(0).Max());

        // Prepare Mobile Module Comparison (all 14 modules)
        MobileModules = new List<ModuleComparisonItemViewModel>();
        foreach (var (moduleName, group) in ReflectProductCatalog.MobileModules)
        {
            var matchingSessions = mobileSessions
                .Where(s => string.Equals(s.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var count = matchingSessions.Count;
            var duration = matchingSessions.Sum(s => s.DurationMinutes);
            var pct = ReflectMobileSessions > 0 ? (count * 100.0 / ReflectMobileSessions) : 0.0;

            MobileModules.Add(new ModuleComparisonItemViewModel
            {
                ModuleName = moduleName,
                ProgramOrGroup = group,
                Platform = ReflectProductCatalog.PlatformMobile,
                SessionCount = count,
                TotalDurationMinutes = duration,
                PercentageOfPlatform = pct
            });
        }
        MobileMaxModuleSessions = Math.Max(1, MobileModules.Select(m => m.SessionCount).DefaultIfEmpty(0).Max());

        // Prepare Hourly Breakdown (0-23 US Eastern Time)
        TimeZoneInfo easternTimeZone;
        try
        {
            easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        }
        catch (TimeZoneNotFoundException)
        {
            easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        }

        var xrByHour = xrSessions
            .Select(s => new
            {
                Session = s,
                EasternTime = TimeZoneInfo.ConvertTime(s.StartedAt, easternTimeZone),
                Program = ReflectProductCatalog.GetGroupOrProgram(ReflectProductCatalog.PlatformXR, s.ModuleName)
            })
            .ToList();

        var mobileByHour = mobileSessions
            .Select(s => new
            {
                Session = s,
                EasternTime = TimeZoneInfo.ConvertTime(s.StartedAt, easternTimeZone),
                Group = ReflectProductCatalog.GetGroupOrProgram(ReflectProductCatalog.PlatformMobile, s.ModuleName)
            })
            .ToList();

        XRHourlyDistribution = new List<HourlyDistributionItemViewModel>();
        MobileHourlyDistribution = new List<HourlyDistributionItemViewModel>();

        for (int hour = 0; hour < 24; hour++)
        {
            string displayLabel = FormatHour(hour);

            // XR hour breakdown
            var xrItem = new HourlyDistributionItemViewModel
            {
                Hour = hour,
                DisplayLabel = displayLabel
            };

            int xrHourTotal = 0;
            foreach (var program in ReflectProductCatalog.XRPrograms)
            {
                int count = xrByHour.Count(x => x.EasternTime.Hour == hour &&
                                                string.Equals(x.Program, program, StringComparison.OrdinalIgnoreCase));
                xrItem.CountsByCategory[program] = count;
                xrHourTotal += count;
            }
            xrItem.TotalCount = xrHourTotal;
            XRHourlyDistribution.Add(xrItem);

            // Mobile hour breakdown
            var mobileItem = new HourlyDistributionItemViewModel
            {
                Hour = hour,
                DisplayLabel = displayLabel
            };

            int mobileHourTotal = 0;
            foreach (var group in ReflectProductCatalog.MobileGroups)
            {
                int count = mobileByHour.Count(m => m.EasternTime.Hour == hour &&
                                                    string.Equals(m.Group, group, StringComparison.OrdinalIgnoreCase));
                mobileItem.CountsByCategory[group] = count;
                mobileHourTotal += count;
            }
            mobileItem.TotalCount = mobileHourTotal;
            MobileHourlyDistribution.Add(mobileItem);
        }

        XRMaxHourlyTotal = Math.Max(1, XRHourlyDistribution.Select(h => h.TotalCount).DefaultIfEmpty(0).Max());
        MobileMaxHourlyTotal = Math.Max(1, MobileHourlyDistribution.Select(h => h.TotalCount).DefaultIfEmpty(0).Max());
    }

    private static string FormatHour(int hour)
    {
        if (hour == 0) return "12 AM";
        if (hour < 12) return $"{hour} AM";
        if (hour == 12) return "12 PM";
        return $"{hour - 12} PM";
    }

    private static double CalculateMedian(IEnumerable<int> values)
    {
        var sorted = values.OrderBy(x => x).ToList();

        if (sorted.Count == 0)
        {
            return 0;
        }

        int middle = sorted.Count / 2;

        if (sorted.Count % 2 == 1)
        {
            return sorted[middle];
        }

        return (sorted[middle - 1] + sorted[middle]) / 2.0;
    }
}
