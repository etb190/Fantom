using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Fantom.Views.Dialogs;

public partial class EditGoalsDialog : Window
{
    public EditGoalsDialog()
    {
        InitializeComponent();
    }

    public EditGoalsDialog(string day, string week, string month, string year) : this()
    {
        DayBox.Text = day;
        WeekBox.Text = week;
        MonthBox.Text = month;
        YearBox.Text = year;
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(null);

    private void OnSave(object? sender, RoutedEventArgs e)
    {
        if (!IsValid(DayBox.Text) || !IsValid(WeekBox.Text) || !IsValid(MonthBox.Text) || !IsValid(YearBox.Text))
        {
            ErrorText.IsVisible = true;
            return;
        }

        Close((
            DayBox.Text!.Trim(),
            WeekBox.Text!.Trim(),
            MonthBox.Text!.Trim(),
            YearBox.Text!.Trim()));
    }

    private static bool IsValid(string? s)
        => double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) && v >= 0;
}
