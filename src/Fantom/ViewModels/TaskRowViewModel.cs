using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fantom.Models;
using Fantom.Services;

namespace Fantom.ViewModels;

/// <summary>One row in the dashboard "Tasks" card.</summary>
public partial class TaskRowViewModel : ObservableObject
{
    private readonly Action<TaskRowViewModel> _startRequested;
    private readonly Action<TaskRowViewModel> _deleteRequested;

    public TaskItem Model { get; }

    public string Name => Model.Name;
    public IBrush ColorBrush { get; }
    public string Initial { get; }

    [ObservableProperty] private string _todayText = "0m";
    [ObservableProperty] private double _sharePercent;
    [ObservableProperty] private bool _isActive;

    public TaskRowViewModel(
        TaskItem model,
        Action<TaskRowViewModel> startRequested,
        Action<TaskRowViewModel> deleteRequested)
    {
        Model = model;
        ColorBrush = BrushFactory.FromHex(model.Color);
        Initial = string.IsNullOrWhiteSpace(model.Name) ? "?" : model.Name[..1].ToUpperInvariant();
        _startRequested = startRequested;
        _deleteRequested = deleteRequested;
    }

    [RelayCommand]
    private void Play() => _startRequested(this);

    [RelayCommand]
    private void Delete() => _deleteRequested(this);
}
