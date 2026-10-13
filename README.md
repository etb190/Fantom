# Fantom ⏱

A lightweight desktop **work time tracker** built with **Avalonia + C# (.NET 8)**.

Start the tracker, leave it running, and stop it when the work is over. Fantom logs every
session to disk in **exactly the same JSON format** as your existing tracker data, so it
drops seamlessly into your workflow.

## Features

- **Running tracker** — pick a task, press *Start Time Tracker*, leave it running, stop it
  when you're done. Switching tasks mid-session automatically closes the current session
  and starts a new one.
- **Dashboard** — live clock, weekly activity %, worked-this-week timer, distinct tasks
  worked this week.
- **Tasks** — create tasks with colors, start tracking directly from a task row, delete
  tasks (past sessions keep the task-name snapshot).
- **Goals** — day / week / month / year hour targets; weekly progress on the dashboard.
- **History** — every session grouped by day with daily totals and a task filter.
- **Import** — point Fantom at a folder containing your existing
  `sessions.json` / `tasks.json` / `timeGoals.json` and it adopts your data.

## Data format

Fantom reads and writes the exact same files as your current tracker:

**`sessions.json`**
```json
[
  {
    "date": "2026-01-08",
    "durationSeconds": 7080,
    "startTime": "2026-01-08T06:02:00.000Z",
    "stopTime": "2026-01-08T08:00:00.000Z",
    "taskId": "d3968783-79bb-40b9-9944-bf2c0fd63ee7",
    "taskName": "Programming"
  }
]
```
- `date` — local (machine timezone) date of the session **start**
- `startTime` / `stopTime` — ISO 8601 UTC with milliseconds
- `taskId` — omitted entirely for sessions without a task; `taskName` is always kept as a
  snapshot, even after the task is deleted

**`tasks.json`**
```json
[
  { "id": "d3968783-79bb-40b9-9944-bf2c0fd63ee7", "name": "Programming", "color": "#2563eb" }
]
```

**`timeGoals.json`** — hour targets stored as strings
```json
{ "dayGoal": "6", "weekGoal": "24", "monthGoal": "96", "yearGoal": "1700" }
```

### Where the data lives

| OS | Location |
|----|----------|
| Windows | `%LOCALAPPDATA%\Fantom\` |
| Linux | `~/.local/share/Fantom/` |
| macOS | `~/.local/share/Fantom/` |

## Getting started

```bash
dotnet run --project src/Fantom
```

Requires the .NET 8 SDK. First launch offers to import your existing data folder —
or choose **Start Fresh** and add tasks from the dashboard.

## Project layout

```
src/Fantom/
├── Models/        # SessionEntry, TaskItem, TimeGoals (exact JSON schema)
├── Services/      # DataStore (persistence/import), TrackerService (running timer), TimeMath
├── ViewModels/    # MVVM view models (CommunityToolkit.Mvvm)
├── Converters/    # value converters for the progress bars
└── Views/         # MainWindow (dashboard + history), dialogs (task/goals/import/confirm)
```

## Notes

- One active session at a time; if the app closes while the tracker is running, the
  session is saved automatically.
- Week-based stats use Monday as the first day of the week.
- Durations are recorded as whole seconds (`durationSeconds`), matching the existing data.
