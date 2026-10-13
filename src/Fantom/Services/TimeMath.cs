using System.Globalization;

namespace Fantom.Services;

/// <summary>Time/date math + formatting helpers used across the app.</summary>
public static class TimeMath
{
    /// <summary>ISO 8601 UTC with milliseconds, e.g. "2026-01-08T06:02:00.000Z".</summary>
    public static string IsoUtc(DateTimeOffset dto)
        => dto.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);

    /// <summary>Local date string "yyyy-MM-dd" of the given instant.</summary>
    public static string LocalDate(DateTimeOffset dto)
        => dto.LocalDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>Parses an ISO 8601 UTC timestamp as written in sessions.json.</summary>
    public static DateTimeOffset ParseIso(string s)
        => DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

    /// <summary>Parses a "yyyy-MM-dd" date string from the "date" field.</summary>
    public static DateTime ParseDate(string d)
        => DateTime.TryParseExact(d, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var dt) ? dt : DateTime.MinValue;

    /// <summary>Parses a goal string ("6") into hours. Returns 0 when invalid.</summary>
    public static double ParseGoal(string? s)
        => double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) && v >= 0 ? v : 0;

    /// <summary>40:00:05 style clock (hours are not capped at 24).</summary>
    public static string FormatClock(long totalSeconds)
    {
        var t = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));
        return $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
    }

    /// <summary>Compact duration: "2h 30m", "45m", "30s".</summary>
    public static string FormatDuration(long totalSeconds)
    {
        var t = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));
        if (t.TotalHours >= 1) return $"{(int)t.TotalHours}h {t.Minutes:00}m";
        if (t.TotalMinutes >= 1) return $"{t.Minutes}m";
        return $"{t.Seconds}s";
    }

    /// <summary>Longer duration: "12h 34m", "34m".</summary>
    public static string FormatDurationLong(long totalSeconds)
    {
        var t = TimeSpan.FromSeconds(Math.Max(0, totalSeconds));
        if (t.TotalHours >= 1) return $"{(int)t.TotalHours}h {t.Minutes:00}m";
        return $"{t.Minutes}m";
    }

    /// <summary>24 -> "24h", 1.5 -> "1.5h".</summary>
    public static string FormatGoal(double hours)
        => hours == Math.Floor(hours)
            ? $"{hours:0}h"
            : $"{hours.ToString("0.#", CultureInfo.InvariantCulture)}h";

    /// <summary>Monday-based start of the week containing the date.</summary>
    public static DateTime StartOfWeek(DateTime date)
        => date.Date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

    /// <summary>"Today", "Yesterday" or "Fri, 12 Sep".</summary>
    public static string DayLabel(DateTime date, DateTime today)
    {
        if (date == today.Date) return "Today";
        if (date == today.Date.AddDays(-1)) return "Yesterday";
        return date.ToString("ddd, dd MMM", CultureInfo.InvariantCulture);
    }
}
