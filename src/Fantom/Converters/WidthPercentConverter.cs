using System.Globalization;
using Avalonia.Data.Converters;

namespace Fantom.Converters;

/// <summary>
/// Multi-value converter: values[0] = track actual width, values[1] = percent.
/// Returns the pixel width the filled bar should have.
/// </summary>
public class WidthPercentConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2
            && values[0] is double trackWidth && !double.IsNaN(trackWidth) && trackWidth > 0
            && values[1] is double pct)
        {
            return Math.Clamp(trackWidth * pct / 100.0, 0, trackWidth);
        }
        return 0d;
    }

    public object ConvertBack(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
