
namespace Reflect.Web.Messages;

public static class ProviderMessages
{
    // Authentication
    public const string DemoClinicianSignedIn = "Demo clinician signed in.";
    public const string SignedOut = "Signed out.";

    // Patients
    public const string PatientFlaggedForReview = "Patient flagged for clinical review.";
    public const string PatientCodeRequired = "Patient code is required.";
    public const string PatientCodeTooLong = "Patient code is too long.";
    public const string InvalidReviewStatus = "Invalid review status.";
    public const string DuplicatePatientCode = "A patient with this code already exists.";
    public const string PatientNotFound = "Patient not found.";
    public const string PatientIdRequired = "A patient ID is required.";

    // Session history
    public const string InvalidDateRange = "Start date cannot be after end date.";

    // Progress
    public const string InvalidDayPeriod = "Only 30 or 60 days are supported.";
    public const string InvalidWeekPeriod = "Only 4-week and 8-week periods are supported.";

    // Clinical response
    public const string ObservationCreated = "Clinical observation created successfully.";

    // Monitoring documentation
    public const string SelectPatientForSummary = "Select a patient to generate a summary.";
    public const string InvalidMonitoringPeriod = "Reporting period must be 30 or 60 days.";
    public const string DraftSaved = "Draft saved successfully.";
    public const string PatientMarkedReviewed = "Patient marked as reviewed.";

    // Reports
    public const string ReportRequestRequired = "A report request is required.";
    public const string InvalidReportFormat = "Format must be pdf or csv.";

    // Overview
    public const string InvalidAttentionThresholds = "Invalid attention queue thresholds.";

    public const string InvalidReportingDateRange =
    "The reporting period start must not be after its end.";

    public const string ReportTemplateUnavailable =
        "The selected report template is not available.";

    public const string InvalidPlatformFilter =
        "Invalid platform filter.";
}
