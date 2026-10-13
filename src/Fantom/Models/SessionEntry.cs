using System.Text.Json.Serialization;

namespace Fantom.Models;

/// <summary>
/// One tracked work session. Serialization format matches the existing
/// sessions.json exactly:
/// { "date", "durationSeconds", "startTime", "stopTime", "taskId", "taskName" }
/// - date:          local (machine timezone) date of the session start, "yyyy-MM-dd"
/// - durationSeconds: active seconds (may be less than the wall-clock span)
/// - startTime/stopTime: ISO 8601 UTC, e.g. "2026-01-08T06:02:00.000Z"
/// - taskId:        omitted entirely when the session has no task
/// - taskName:      kept as a snapshot even when the task no longer exists
/// </summary>
public class SessionEntry
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("durationSeconds")]
    public long DurationSeconds { get; set; }

    [JsonPropertyName("startTime")]
    public string StartTime { get; set; } = "";

    [JsonPropertyName("stopTime")]
    public string StopTime { get; set; } = "";

    [JsonPropertyName("taskId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TaskId { get; set; }

    [JsonPropertyName("taskName")]
    public string TaskName { get; set; } = "";
}
