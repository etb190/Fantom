using Avalonia.Media;
using Fantom.Models;
using Fantom.Services;

namespace Fantom.ViewModels;

/// <summary>One session row inside a day group of the activity lists.</summary>
public class SessionRowViewModel
{
    public string TaskName { get; }
    public IBrush DotBrush { get; }
    public string TimeRangeText { get; }
    public string DurationText { get; }

    public SessionRowViewModel(SessionEntry s, IReadOnlyDictionary<string, TaskItem> tasksById)
    {
        TaskName = string.IsNullOrWhiteSpace(s.TaskName) ? "(No task)" : s.TaskName;

        DotBrush = s.TaskId != null && tasksById.TryGetValue(s.TaskId, out var t)
            ? BrushFactory.FromHex(t.Color)
            : BrushFactory.FromHex("#cbd5e1");

        var start = TimeMath.ParseIso(s.StartTime).ToLocalTime();
        var stop = TimeMath.ParseIso(s.StopTime).ToLocalTime();
        TimeRangeText = $"{start:h:mm tt} – {stop:h:mm tt}";
        DurationText = TimeMath.FormatDuration(s.DurationSeconds);
    }
}
