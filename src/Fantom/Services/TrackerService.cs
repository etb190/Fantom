using Avalonia.Threading;
using Fantom.Models;

namespace Fantom.Services;

/// <summary>
/// The running tracker. You start it, leave it running, and stop it when the
/// work is over. Exactly one session can be active at a time; switching tasks
/// while running is handled by the ViewModel (stop + save + start new).
/// The 1s tick always runs so the dashboard clock stays live.
/// </summary>
public class TrackerService : IDisposable
{
    private readonly DispatcherTimer _timer;

    public bool IsRunning { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public TaskItem? ActiveTask { get; private set; }

    /// <summary>Raised every second (used for the clock, elapsed time, live stats).</summary>
    public event Action? Tick;

    public TrackerService()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => Tick?.Invoke();
        _timer.Start();
    }

    public long ElapsedSeconds
        => IsRunning ? (long)(DateTimeOffset.UtcNow - StartTime).TotalSeconds : 0;

    public void Start(TaskItem? task)
    {
        if (IsRunning) return;
        ActiveTask = task;
        StartTime = DateTimeOffset.UtcNow;
        IsRunning = true;
    }

    /// <summary>
    /// Stops the tracker and produces the session entry in the exact
    /// sessions.json format. Does NOT persist — the caller saves via DataStore.
    /// </summary>
    public SessionEntry Stop()
    {
        var stop = DateTimeOffset.UtcNow;
        var start = StartTime;
        IsRunning = false;

        var entry = new SessionEntry
        {
            Date = TimeMath.LocalDate(start),
            DurationSeconds = (long)(stop - start).TotalSeconds,
            StartTime = TimeMath.IsoUtc(start),
            StopTime = TimeMath.IsoUtc(stop),
            TaskId = ActiveTask?.Id,
            TaskName = ActiveTask?.Name ?? ""
        };
        ActiveTask = null;
        return entry;
    }

    public void Dispose() => _timer.Stop();
}
