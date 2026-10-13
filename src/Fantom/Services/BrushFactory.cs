using Avalonia.Media;

namespace Fantom.Services;

/// <summary>Parses "#2563eb" style hex strings into brushes, with a safe fallback.</summary>
public static class BrushFactory
{
    public static IBrush FromHex(string? hex)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(hex))
                return new SolidColorBrush(Color.Parse(hex.Trim())).ToImmutable();
        }
        catch
        {
            // fall through to default
        }
        return new SolidColorBrush(Color.Parse("#2563eb")).ToImmutable();
    }
}
