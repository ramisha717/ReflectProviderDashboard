
using System.Globalization;

namespace Reflect.Web.Extensions;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo EasternTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

    public static string ToUsDateString(this DateTimeOffset value)
    {
        var easternTime = TimeZoneInfo.ConvertTime(
            value,
            EasternTimeZone);

        return easternTime.ToString(
            "M/d/yyyy",
            CultureInfo.InvariantCulture);
    }

    public static string ToUsDateTimeString(this DateTimeOffset value)
    {
        var easternTime = TimeZoneInfo.ConvertTime(
            value,
            EasternTimeZone);

        return easternTime.ToString(
            "M/d/yyyy h:mm tt",
            CultureInfo.InvariantCulture);
    }
}
