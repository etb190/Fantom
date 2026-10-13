using Avalonia.Media;
using Fantom.Models;
using Fantom.Services;

namespace Fantom.ViewModels;

/// <summary>Result of the Add Task dialog: name + chosen hex color.</summary>
public record TaskDraft(string Name, string Color);

/// <summary>
/// An entry in the task selector ComboBox. Also used for the special
/// "(No task)" and "All tasks" pseudo-options.
/// </summary>
public class TaskOption
{
    public string? Id { get; init; }
    public string Name { get; init; } = "";
    public IBrush Brush { get; init; } = BrushFactory.FromHex("#94a3b8");
    public bool IsNone { get; init; }
    public bool IsAll { get; init; }

    public static TaskOption None { get; } = new()
    {
        Id = null, Name = "(No task)", IsNone = true, Brush = BrushFactory.FromHex("#cbd5e1")
    };

    public static TaskOption All { get; } = new()
    {
        Id = null, Name = "All tasks", IsAll = true, Brush = BrushFactory.FromHex("#94a3b8")
    };

    public static TaskOption FromTask(TaskItem t) => new()
    {
        Id = t.Id, Name = t.Name, Brush = BrushFactory.FromHex(t.Color)
    };
}
