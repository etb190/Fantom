using System.Text.Json.Serialization;

namespace Fantom.Models;

/// <summary>
/// Time goals in hours. Serialization format matches the existing
/// timeGoals.json exactly — the values are stored as STRINGS, e.g. "6".
/// </summary>
public class TimeGoals
{
    [JsonPropertyName("dayGoal")]
    public string DayGoal { get; set; } = "6";

    [JsonPropertyName("weekGoal")]
    public string WeekGoal { get; set; } = "24";

    [JsonPropertyName("monthGoal")]
    public string MonthGoal { get; set; } = "96";

    [JsonPropertyName("yearGoal")]
    public string YearGoal { get; set; } = "1700";
}
