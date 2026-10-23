namespace Reflect.Web.Models.ViewModels.Patients.Profile;

public class PatientProfileWeeklyEngagementViewModel
{
    public DateTimeOffset WeekStart { get; set; }
    public int ReflectXRMinutes { get; set; }
    public int ReflectMobileMinutes { get; set; }
}