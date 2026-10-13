using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Fantom.Views.Dialogs;

public enum ImportOutcome
{
    Fresh,
    Folder
}

public partial class ImportDialog : Window
{
    /// <summary>The folder picked by the user, valid only when result == Folder.</summary>
    public string? ImportedFolder { get; private set; }

    public ImportDialog()
    {
        InitializeComponent();
    }

    /// <summary>Reuses the same dialog for a manual import from the dashboard.</summary>
    public ImportDialog(bool manualImport) : this()
    {
        Title = "Import Data";
        TitleText.Text = "Import Data";
        BodyText.Text = "Pick the folder that contains your existing sessions.json, tasks.json and timeGoals.json. " +
                        "The imported files replace the data currently stored by Fantom.";
    }

    private async void OnChooseFolder(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose the folder that contains your tracker data",
            AllowMultiple = false
        });

        if (folders.Count == 0) return;

        var path = folders[0].Path.LocalPath;
        var hasSessions = File.Exists(Path.Combine(path, "sessions.json"));
        var hasTasks = File.Exists(Path.Combine(path, "tasks.json"));

        if (!hasSessions && !hasTasks)
        {
            ErrorText.Text = "That folder has no sessions.json or tasks.json in it.";
            ErrorText.IsVisible = true;
            return;
        }

        ImportedFolder = path;
        Close(ImportOutcome.Folder);
    }

    private void OnStartFresh(object? sender, RoutedEventArgs e) => Close(ImportOutcome.Fresh);
}
