using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Fantom.Services;
using Fantom.ViewModels;

namespace Fantom.Views.Dialogs;

public partial class AddTaskDialog : Window
{
    private static readonly string[] Palette =
    {
        "#2563eb", "#0ea5e9", "#059669", "#65a30d", "#d97706",
        "#ea580c", "#dc2626", "#db2777", "#7c3aed", "#475569"
    };

    private string _selectedColor = Palette[0];

    public AddTaskDialog()
    {
        InitializeComponent();

        foreach (var color in Palette)
        {
            var btn = new Button
            {
                Classes = { "swatch" },
                Background = BrushFactory.FromHex(color),
                Tag = color
            };
            btn.Click += OnSwatchClick;
            SwatchesHost.Children.Add(btn);
        }

        MarkSelected();
        NameBox.AttachedToVisualTree += (_, _) => NameBox.Focus();
    }

    private void MarkSelected()
    {
        foreach (var child in SwatchesHost.Children)
        {
            if (child is Button { Tag: string color } btn)
            {
                if (color == _selectedColor)
                {
                    btn.BorderBrush = new SolidColorBrush(Color.Parse("#0F172A"));
                    btn.BorderThickness = new Avalonia.Thickness(2.5);
                }
                else
                {
                    btn.BorderThickness = new Avalonia.Thickness(0);
                }
            }
        }
    }

    private void OnSwatchClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string color })
        {
            _selectedColor = color;
            MarkSelected();
        }
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(null);

    private void OnSave(object? sender, RoutedEventArgs e)
    {
        var name = NameBox.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            ErrorText.IsVisible = true;
            return;
        }
        Close(new TaskDraft(name, _selectedColor));
    }
}
