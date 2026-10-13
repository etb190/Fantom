using Avalonia.Controls;
using Fantom.ViewModels;
using Fantom.Views.Dialogs;

namespace Fantom.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel Vm => (MainWindowViewModel)DataContext!;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        Vm.AddTaskRequested += OnAddTaskRequested;
        Vm.EditGoalsRequested += OnEditGoalsRequested;
        Vm.ImportRequested += OnImportRequested;
        Vm.DeleteRequested += OnDeleteRequested;

        Opened += (_, _) => Vm.Initialize();
        Closing += (_, _) => Vm.HandleAppClosing();
        Closed += (_, _) => Vm.Dispose();
    }

    private async void OnAddTaskRequested()
    {
        var dlg = new AddTaskDialog();
        var draft = await dlg.ShowDialog<TaskDraft?>(this);
        if (draft != null)
            Vm.CompleteAddTask(draft.Name, draft.Color);
    }

    private async void OnEditGoalsRequested()
    {
        var g = Vm.CurrentGoals;
        var dlg = new EditGoalsDialog(g.DayGoal, g.WeekGoal, g.MonthGoal, g.YearGoal);
        var result = await dlg.ShowDialog<(string Day, string Week, string Month, string Year)?>(this);
        if (result.HasValue)
            Vm.CompleteEditGoals(result.Value.Day, result.Value.Week, result.Value.Month, result.Value.Year);
    }

    private async void OnImportRequested()
    {
        var dlg = new ImportDialog();
        var outcome = await dlg.ShowDialog<ImportOutcome?>(this);

        if (outcome == ImportOutcome.Fresh)
        {
            Vm.StartFresh();
            return;
        }
        if (outcome == ImportOutcome.Folder && dlg.ImportedFolder != null)
        {
            var res = Vm.ImportFrom(dlg.ImportedFolder);
            if (!res.Success)
                await new MessageBox("Import failed", res.Message, "OK").ShowDialog<bool>(this);
        }
    }

    private async void OnDeleteRequested(TaskRowViewModel row)
    {
        var ok = await new MessageBox(
            "Delete task",
            $"Delete \u201C{row.Name}\u201D?\n\nPast sessions are kept \u2014 they will still show the task name.",
            "Delete").ShowDialog<bool>(this);
        if (ok)
            Vm.ConfirmDelete(row);
    }
}
