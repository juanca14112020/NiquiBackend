using System.Text;

namespace NiquiBackend.Infrastructure.Common;

public static class ColombiaTimeHelper
{
    private static readonly TimeZoneInfo ColombiaTimeZone = GetColombiaTimeZone();

    private static TimeZoneInfo GetColombiaTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
        }
        catch
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        }
    }

    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ColombiaTimeZone);

    public static DateTime ConvertToColombiaTime(DateTime utcDateTime)
    {
        var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, ColombiaTimeZone);
    }
}