using System.Globalization;
using Microsoft.Maui.Controls;

namespace ChibitsLink.main.cs.converters;

/// <summary>
/// Convierte un booleano (victoria/derrota) a texto de resultado.
/// </summary>
public class BooleanToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (bool)(value ?? false) ? "VICTORIA" : "DERROTA";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
