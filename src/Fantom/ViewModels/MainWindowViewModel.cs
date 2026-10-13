using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fantom.Models;
using Fantom.Services;

namespace Fantom.ViewModels;

/// <summary>
/// Main orchestrator: dashboard stats, running tracker, task management,
/// history view, goals, import. Dialogs are opened by the View via events.
/// </summary>
public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly DataStore _store = new();
    private readonly TrackerService _tracker = new();

    public event Action? AddTaskRequested;
    public event Action? EditGoalsRequested;
    public event Action? ImportRequested;
    public event Action<TaskRowViewModel>? DeleteRequested;

    public MainWindowViewModel()
    {
        _store.Load();
        _tracker.Tick += OnTick;
    }

    /// <summary>Call after the window has subscribed to the events.</summary>
    public void Initialize()
    {
        RebuildAll();
        if (!_store.HasData)
            ImportRequested?.Invoke();
    }

    // ---------------------------------------------------------------- state

    public ObservableCollection<TaskOption> TaskOptions { get; } = new();
    public ObservableCollection<TaskOption> HistoryFilterOptions { get; } = new();
    public ObservableCollection<TaskRowViewModel> TaskRows { get; } = new();
    public ObservableCollection<DayGroupViewModel> RecentGroups { get; } = new();
    public ObservableCollection<DayGroupViewModel> HistoryGroups { get; } = new();

    [ObservableProperty] private TaskOption _selectedTaskOption = TaskOption.None;
    [ObservableProperty] private TaskOption _selectedHistoryFilter = TaskOption.All;
    [ObservableProperty] private bool _isHistoryOpen;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _headerClock = "";
    [ObservableProperty] private string _startStopLabel = "Start Time Tracker";
    [ObservableProperty] private string _activeElapsedText = "";
    [ObservableProperty] private string _weekActivityPercent = "0%";
    [ObservableProperty] private string _weekActivitySub = "no goal set";
    [ObservableProperty] private string _weekWorkedText = "00:00:00";
    [ObservableProperty] private string _weekWorkedSub = "this week";
    [ObservableProperty] private string _tasksWorkedText = "00";
    [ObservableProperty] private string _historySummary = "";
    [ObservableProperty] private bool _hasRecentData;
    [ObservableProperty] private bool _hasTaskRows;

    partial void OnSelectedHistoryFilterChanged(TaskOption value) => RebuildHistory();

    public TimeGoals CurrentGoals => _store.Goals;

    // ------------------------------------------------------------ commands

    [RelayCommand]
    private void StartStop()
    {
        if (_tracker.IsRunning)
        {
            StopTracker(save: true);
        }
        else
        {
            _tracker.Start(ResolveTask(SelectedTaskOption));
            IsRunning = true;
            RefreshHeader();
            RefreshTaskRows();
        }
    }

    [RelayCommand]
    private void StartTask(TaskRowViewModel? row)
    {
        if (row == null) return;
        var task = _store.Tasks.FirstOrDefault(t => t.Id == row.Model.Id);
        if (task == null) return;

        if (_tracker.IsRunning)
        {
            if (_tracker.ActiveTask?.Id == task.Id) return; // already tracking it
            StopTracker(save: true); // close the running session first
        }

        _tracker.Start(task);
        IsRunning = true;

        var opt = TaskOptions.FirstOrDefault(o => o.Id == task.Id);
        if (opt != null) SelectedTaskOption = opt;

        RefreshHeader();
        RefreshTaskRows();
    }

    [RelayCommand]
    private void RequestDelete(TaskRowViewModel? row)
    {
        if (row != null) DeleteRequested?.Invoke(row);
    }

    /// <summary>Called by the View after the user confirms the delete.</summary>
    public void ConfirmDelete(TaskRowViewModel row)
    {
        if (_tracker.IsRunning && _tracker.ActiveTask?.Id == row.Model.Id)
            StopTracker(save: true);

        var task = _store.Tasks.FirstOrDefault(t => t.Id == row.Model.Id);
        if (task != null)
        {
            _store.Tasks.Remove(task);
            _store.SaveTasks();
        }
        RebuildAll();
    }

    [RelayCommand]
    private void AddTask() => AddTaskRequested?.Invoke();

    /// <summary>Called by the View with the values entered in the Add Task dialog.</summary>
    public void CompleteAddTask(string name, string colorHex)
    {
        name = name.Trim();
        if (name.Length == 0) return;
        _store.Tasks.Add(new TaskItem { Id = Guid.NewGuid().ToString(), Name = name, Color = colorHex });
        _store.SaveTasks();
        RebuildAll();
    }

    [RelayCommand]
    private void EditGoals() => EditGoalsRequested?.Invoke();

    /// <summary>Called by the View with the values entered in the Goals dialog.</summary>
    public void CompleteEditGoals(string day, string week, string month, string year)
    {
        _store.SetGoals(new TimeGoals { DayGoal = day, WeekGoal = week, MonthGoal = month, YearGoal = year });
        _store.SaveGoals();
        RefreshAllStats();
    }

    [RelayCommand]
    private void Import() => ImportRequested?.Invoke();

    /// <summary>Called by the View after a folder was picked in the import dialog.</summary>
    public ImportResult ImportFrom(string folder) => _store.ImportFromFolder(folder);

    /// <summary>Called by the View when the user chooses to start without importing.</summary>
    public void StartFresh()
    {
        _store.EnsureDefaults();
        RebuildAll();
    }

    [RelayCommand]
    private void ViewAll()
    {
        RebuildHistory();
        IsHistoryOpen = true;
    }

    [RelayCommand]
    private void Back() => IsHistoryOpen = false;

    /// <summary>Called when the app window closes: never lose a running session.</summary>
    public void HandleAppClosing()
    {
        if (_tracker.IsRunning)
            StopTracker(save: true);
    }

    public void Dispose()
    {
        _tracker.Tick -= OnTick;
        _tracker.Dispose();
    }

    // ----------------------------------------------------------- internals

    private TaskItem? ResolveTask(TaskOption? opt)
    {
        if (opt?.Id == null) return null;
        return _store.Tasks.FirstOrDefault(t => t.Id == opt.Id);
    }

    private void StopTracker(bool save)
    {
        if (!_tracker.IsRunning) return;

        var entry = _tracker.Stop();
        IsRunning = false;

        if (save)
        {
            _store.Sessions.Add(entry);
            _store.SaveSessions();
        }

        ActiveElapsedText = "";
        RebuildAll();
    }

    private void OnTick()
    {
        RefreshHeader();
        if (_tracker.IsRunning)
        {
            RefreshAllStats();
            UpdateActiveTaskRow();
        }
    }

    private void RefreshHeader()
    {
        HeaderClock = DateTime.Now.ToString("ddd, dd MMM yyyy | h:mm tt", CultureInfo.InvariantCulture);
        StartStopLabel = _tracker.IsRunning ? "Stop Time Tracker" : "Start Time Tracker";
        if (_tracker.IsRunning)
            ActiveElapsedText = TimeMath.FormatClock(_tracker.ElapsedSeconds);
    }

    private void RebuildAll()
    {
        RebuildTaskOptions();
        RebuildTaskRows();
        RebuildRecent();
        RebuildHistory();
        RefreshAllStats();
        RefreshHeader();
    }

    private void RebuildTaskOptions()
    {
        var previousTaskId = SelectedTaskOption?.Id;
        var previousFilterId = SelectedHistoryFilter?.Id;
        var previousFilterIsAll = SelectedHistoryFilter is { IsAll: true };

        TaskOptions.Clear();
        TaskOptions.Add(TaskOption.None);
        foreach (var t in _store.Tasks)
            TaskOptions.Add(TaskOption.FromTask(t));

        HistoryFilterOptions.Clear();
        HistoryFilterOptions.Add(TaskOption.All);
        foreach (var t in _store.Tasks)
            HistoryFilterOptions.Add(TaskOption.FromTask(t));

        // Force the ComboBoxes to re-resolve their selection after the refill.
        // Assigning the same reference would be a no-op and leave them blank.
#pragma warning disable MVVMTK0034 // deliberate null-out + reassign to force selection change
        _selectedTaskOption = null!;
        OnPropertyChanged(nameof(SelectedTaskOption));
        SelectedTaskOption = previousTaskId != null
            ? TaskOptions.FirstOrDefault(o => o.Id == previousTaskId) ?? TaskOption.None
            : TaskOption.None;

        _selectedHistoryFilter = null!;
        OnPropertyChanged(nameof(SelectedHistoryFilter));
        SelectedHistoryFilter = previousFilterIsAll || previousFilterId == null
            ? TaskOption.All
            : HistoryFilterOptions.FirstOrDefault(o => o.Id == previousFilterId) ?? TaskOption.All;
#pragma warning restore MVVMTK0034
    }

    private void RebuildTaskRows()
    {
        TaskRows.Clear();
        foreach (var t in _store.Tasks)
            TaskRows.Add(new TaskRowViewModel(
                t,
                startRequested: row => StartTask(row),
                deleteRequested: row => RequestDelete(row)));

        HasTaskRows = TaskRows.Count > 0;
        RefreshTaskRows();
    }

    private void RefreshTaskRows()
    {
        var activeId = _tracker.ActiveTask?.Id;
        var todayTotals = TodaySecondsPerTask();

        foreach (var row in TaskRows)
        {
            row.IsActive = activeId != null && row.Model.Id == activeId;

            var secs = todayTotals.GetValueOrDefault(row.Model.Id);
            if (row.IsActive) secs += _tracker.ElapsedSeconds;
            row.TodayText = TimeMath.FormatDuration(secs);

            var today = TodaySeconds() + _tracker.ElapsedSeconds;
            row.SharePercent = today > 0 ? Math.Clamp(secs * 100.0 / today, 0, 100) : 0;
        }
    }

    private void UpdateActiveTaskRow()
    {
        var activeId = _tracker.ActiveTask?.Id;
        if (activeId == null) return;

        var todayTotals = TodaySecondsPerTask();
        var today = TodaySeconds() + _tracker.ElapsedSeconds;

        foreach (var row in TaskRows.Where(r => r.Model.Id == activeId))
        {
            row.IsActive = true;
            var secs = todayTotals.GetValueOrDefault(row.Model.Id) + _tracker.ElapsedSeconds;
            row.TodayText = TimeMath.FormatDuration(secs);
            row.SharePercent = today > 0 ? Math.Clamp(secs * 100.0 / today, 0, 100) : 0;
        }
    }

    private void RebuildRecent()
    {
        RecentGroups.Clear();
        foreach (var g in BuildGroups(_store.Sessions, takeDays: 10))
            RecentGroups.Add(g);
        HasRecentData = RecentGroups.Count > 0;
    }

    private void RebuildHistory()
    {
        string? filterId = null;
        if (SelectedHistoryFilter is { IsAll: false } f) filterId = f.Id;

        HistoryGroups.Clear();
        foreach (var g in BuildGroups(_store.Sessions, takeDays: null, filterTaskId: filterId))
            HistoryGroups.Add(g);

        var filtered = _store.Sessions
            .Where(s => filterId == null || s.TaskId == filterId)
            .ToList();
        var total = filtered.Sum(s => s.DurationSeconds);
        HistorySummary = $"{filtered.Count} sessions · {TimeMath.FormatDurationLong(total)} total";
    }

    private List<DayGroupViewModel> BuildGroups(
        IReadOnlyList<SessionEntry> sessions, int? takeDays, string? filterTaskId = null)
    {
        var tasksById = _store.Tasks.ToDictionary(t => t.Id);

        IEnumerable<SessionEntry> query = sessions;
        if (filterTaskId != null)
            query = query.Where(s => s.TaskId == filterTaskId);

        var groups = query
            .GroupBy(s => s.Date)
            .OrderByDescending(g => g.Key, StringComparer.Ordinal)
            .ToList();

        if (takeDays.HasValue)
            groups = groups.Take(takeDays.Value).ToList();

        var today = DateTime.Today;
        var result = new List<DayGroupViewModel>();

        foreach (var g in groups)
        {
            var rows = g
                .OrderByDescending(s => s.StartTime, StringComparer.Ordinal)
                .Select(s => new SessionRowViewModel(s, tasksById))
                .ToList();

            var total = g.Sum(s => s.DurationSeconds);
            result.Add(new DayGroupViewModel(
                TimeMath.DayLabel(TimeMath.ParseDate(g.Key), today),
                TimeMath.FormatDurationLong(total),
                rows));
        }

        return result;
    }

    private void RefreshAllStats()
    {
        var weekGoal = TimeMath.ParseGoal(_store.Goals.WeekGoal);
        var weekSec = WeekSeconds() + (_tracker.IsRunning ? _tracker.ElapsedSeconds : 0);

        var pct = weekGoal > 0 ? weekSec / (weekGoal * 3600.0) * 100.0 : 0;
        WeekActivityPercent = $"{Math.Min(pct, 999):0}%";
        WeekActivitySub = weekGoal > 0
            ? $"{TimeMath.FormatDurationLong(weekSec)} of {TimeMath.FormatGoal(weekGoal)} goal"
            : $"{TimeMath.FormatDurationLong(weekSec)} · no goal set";

        WeekWorkedText = TimeMath.FormatClock(weekSec);
        WeekWorkedSub = weekGoal > 0
            ? $"of {TimeMath.FormatGoal(weekGoal)} goal"
            : "this week";

        var distinct = DistinctTasksThisWeek();
        if (_tracker.IsRunning && _tracker.ActiveTask != null)
        {
            var weekStart = TimeMath.StartOfWeek(DateTime.Today);
            var anySessionThisWeek = _store.Sessions.Any(s =>
                s.TaskId == _tracker.ActiveTask.Id &&
                TimeMath.ParseDate(s.Date) >= weekStart);
            if (!anySessionThisWeek) distinct++;
        }
        TasksWorkedText = distinct.ToString("00");
    }

    private long TodaySeconds()
    {
        var today = DateTime.Today;
        return _store.Sessions
            .Where(s => TimeMath.ParseDate(s.Date) == today)
            .Sum(s => s.DurationSeconds);
    }

    private long WeekSeconds()
    {
        var weekStart = TimeMath.StartOfWeek(DateTime.Today);
        var weekEnd = weekStart.AddDays(7);
        return _store.Sessions
            .Where(s => TimeMath.ParseDate(s.Date) >= weekStart && TimeMath.ParseDate(s.Date) < weekEnd)
            .Sum(s => s.DurationSeconds);
    }

    private int DistinctTasksThisWeek()
    {
        var weekStart = TimeMath.StartOfWeek(DateTime.Today);
        var weekEnd = weekStart.AddDays(7);
        return _store.Sessions
            .Where(s => s.TaskId != null &&
                        TimeMath.ParseDate(s.Date) >= weekStart &&
                        TimeMath.ParseDate(s.Date) < weekEnd)
            .Select(s => s.TaskId)
            .Distinct(StringComparer.Ordinal)
            .Count();
    }

    private Dictionary<string, long> TodaySecondsPerTask()
    {
        var today = DateTime.Today;
        var dict = new Dictionary<string, long>();
        foreach (var s in _store.Sessions)
        {
            if (s.TaskId == null || TimeMath.ParseDate(s.Date) != today) continue;
            dict[s.TaskId] = dict.GetValueOrDefault(s.TaskId) + s.DurationSeconds;
        }
        return dict;
    }
}
