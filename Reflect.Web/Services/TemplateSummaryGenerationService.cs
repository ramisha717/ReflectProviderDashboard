
using System.Globalization;
using Reflect.Domain.Entities;
using Reflect.Domain.Services;

namespace Reflect.Web.Services;

public class TemplateSummaryGenerationService : ISummaryGenerationService
{
  public string GenerateSummary(
    IReadOnlyList<PatientSession> sessions,
    bool includeSessionCountAndDuration,
    bool includePlatformActivity,
    bool includeModuleActivity,
    bool includeClinicianObservations,
    IReadOnlyList<ClinicalObservation> clinicalObservations)
  {
    var sections = new List<string>();

    if (includeSessionCountAndDuration)
    {
        sections.Add(GenerateSessionSummary(sessions));
    }

    if (includePlatformActivity)
    {
        sections.Add(GeneratePlatformSummary(sessions));
    }

    if (includeModuleActivity)
    {
        sections.Add(GenerateModuleSummary(sessions));
    }

    if (includeClinicianObservations)
    {
        sections.Add(GenerateObservationSummary(clinicalObservations));
    }

    return string.Join("\n\n", sections);
  }

  private static string GenerateSessionSummary(
    IReadOnlyList<PatientSession> sessions)
  {
    if (sessions.Count == 0)
    {
      return "No sessions were recorded during the selected period.";
    }

    var durations = sessions
      .Select(s => s.DurationMinutes)
      .OrderBy(d => d)
      .ToList();

    int count = durations.Count;

    double median = count % 2 == 1
      ? durations[count / 2]
      : ((double)durations[count / 2 - 1] + durations[count / 2]) / 2;

    return $"A total of {count} sessions were recorded during the selected period. " +
      $"The median session duration was {median.ToString("0.#", CultureInfo.InvariantCulture)} minutes.";
  }

  private static string GeneratePlatformSummary(
    IReadOnlyList<PatientSession> sessions)
  {
    int xrCount = sessions.Count(s =>
      string.Equals(s.Platform, "Reflect XR", StringComparison.OrdinalIgnoreCase));

    int mobileCount = sessions.Count(s =>
       string.Equals(s.Platform, "Reflect Mobile", StringComparison.OrdinalIgnoreCase));

    return $"{xrCount} Reflect XR sessions and {mobileCount} Reflect Mobile sessions " +
      "were recorded during the selected period.";
  }

  private static string GenerateModuleSummary(
      IReadOnlyList<PatientSession> sessions)
  {
      var modules = sessions
        .Where(s => !string.IsNullOrWhiteSpace(s.ModuleName))
        .GroupBy(s => s.ModuleName, StringComparer.OrdinalIgnoreCase)
        .OrderByDescending(g => g.Count())
        .ThenBy(g => g.Key)
        .Select(g => $"{g.Key} ({g.Count()} sessions)")
        .ToList();

      if (modules.Count == 0)
      {
        return "No module activity was recorded during the selected period.";
      }

      return "Recorded module activity included: " +
        string.Join(", ", modules) + ".";
  }


  private static string GenerateObservationSummary(
    IReadOnlyList<ClinicalObservation> clinicalObservations)
  {
    var notes = clinicalObservations
      .Where(o => !string.IsNullOrWhiteSpace(o.Notes))
      .OrderBy(o => o.ObservedAt)
      .Select(o => o.Notes)
      .ToList();

    if (notes.Count == 0)
    {
      return "No clinical observations were recorded during the selected period.";
    }

    return "Clinical observations:\n" + string.Join("\n", notes);
  }


}
