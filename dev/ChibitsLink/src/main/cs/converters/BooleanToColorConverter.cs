using System.Globalization;
using Microsoft.Maui.Controls;

namespace ChibitsLink.main.cs.converters;

/// <summary>
/// Convierte un booleano (victoria/derrota) a color de resultado.
/// </summary>
public class BooleanToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (bool)(value ?? false) ? Color.FromArgb("#00B894") : Color.FromArgb("#FF6584");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
