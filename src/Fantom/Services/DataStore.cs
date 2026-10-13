using System.IO;
using System.Text.Json;
using Fantom.Models;

namespace Fantom.Services;

public class ImportResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
}

/// <summary>
/// Loads and saves the three data files (sessions.json, tasks.json, timeGoals.json)
/// in exactly the same format as the user's existing tracker data.
/// Files live in %LOCALAPPDATA%/Fantom (platform equivalent).
/// </summary>
public class DataStore
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true // 2-space indent, matches the original files
    };

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string Folder { get; }
    public string SessionsPath => Path.Combine(Folder, "sessions.json");
    public string TasksPath => Path.Combine(Folder, "tasks.json");
    public string GoalsPath => Path.Combine(Folder, "timeGoals.json");

    public List<SessionEntry> Sessions { get; private set; } = new();
    public List<TaskItem> Tasks { get; private set; } = new();
    public TimeGoals Goals { get; private set; } = new();

    /// <summary>True when sessions.json + tasks.json exist on disk.</summary>
    public bool HasData => File.Exists(SessionsPath) && File.Exists(TasksPath);

    public DataStore()
    {
        Folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Fantom");
        Directory.CreateDirectory(Folder);
    }

    public void Load()
    {
        if (File.Exists(SessionsPath))
            Sessions = DeserializeList<SessionEntry>(SessionsPath) ?? new List<SessionEntry>();

        if (File.Exists(TasksPath))
            Tasks = DeserializeList<TaskItem>(TasksPath) ?? new List<TaskItem>();

        if (File.Exists(GoalsPath))
            Goals = DeserializeOne<TimeGoals>(GoalsPath) ?? new TimeGoals();
        else
        {
            Goals = new TimeGoals();
            SaveGoals();
        }
    }

    private static List<T>? DeserializeList<T>(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path), ReadOptions);
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            return null;
        }
    }

    private static T? DeserializeOne<T>(string path) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path), ReadOptions);
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            return null;
        }
    }

    public void SaveSessions()
    {
        // Keep the file chronologically ascending, like the original data.
        Sessions.Sort((a, b) => string.CompareOrdinal(a.StartTime, b.StartTime));
        File.WriteAllText(SessionsPath, JsonSerializer.Serialize(Sessions, WriteOptions));
    }

    public void SaveTasks()
        => File.WriteAllText(TasksPath, JsonSerializer.Serialize(Tasks, WriteOptions));

    public void SetGoals(TimeGoals goals)
        => Goals = goals;

    public void SaveGoals()
        => File.WriteAllText(GoalsPath, JsonSerializer.Serialize(Goals, WriteOptions));

    /// <summary>Creates empty sessions/tasks files (goals already default).</summary>
    public void EnsureDefaults()
    {
        if (!File.Exists(SessionsPath)) SaveSessions();
        if (!File.Exists(TasksPath)) SaveTasks();
        if (!File.Exists(GoalsPath)) SaveGoals();
    }

    /// <summary>
    /// Imports the three data files from a folder. Each file is validated by
    /// parsing it, then copied verbatim so the original formatting is preserved.
    /// Imported files replace the current data.
    /// </summary>
    public ImportResult ImportFromFolder(string sourceFolder)
    {
        int sessions = 0, tasks = 0;
        bool goals = false, any = false;

        var sPath = Path.Combine(sourceFolder, "sessions.json");
        if (File.Exists(sPath))
        {
            var parsed = DeserializeList<SessionEntry>(sPath);
            if (parsed == null)
                return new ImportResult { Success = false, Message = "sessions.json could not be parsed." };
            File.Copy(sPath, SessionsPath, overwrite: true);
            sessions = parsed.Count;
            any = true;
        }

        var tPath = Path.Combine(sourceFolder, "tasks.json");
        if (File.Exists(tPath))
        {
            var parsed = DeserializeList<TaskItem>(tPath);
            if (parsed == null)
                return new ImportResult { Success = false, Message = "tasks.json could not be parsed." };
            File.Copy(tPath, TasksPath, overwrite: true);
            tasks = parsed.Count;
            any = true;
        }

        var gPath = Path.Combine(sourceFolder, "timeGoals.json");
        if (File.Exists(gPath))
        {
            var parsed = DeserializeOne<TimeGoals>(gPath);
            if (parsed == null)
                return new ImportResult { Success = false, Message = "timeGoals.json could not be parsed." };
            File.Copy(gPath, GoalsPath, overwrite: true);
            goals = true;
            any = true;
        }

        if (!any)
            return new ImportResult
            {
                Success = false,
                Message = "No sessions.json / tasks.json / timeGoals.json found in that folder."
            };

        Load();

        var parts = new List<string>();
        if (sessions > 0 || File.Exists(SessionsPath)) parts.Add($"{sessions} sessions");
        if (tasks > 0 || File.Exists(TasksPath)) parts.Add($"{tasks} tasks");
        if (goals) parts.Add("goals");

        return new ImportResult
        {
            Success = true,
            Message = $"Imported {string.Join(", ", parts)}."
        };
    }
}
