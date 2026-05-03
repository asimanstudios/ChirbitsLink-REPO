using System.Globalization;
using Microsoft.Maui.Controls;
// TODO- VALORAR SI ES 100% NECESARIO
namespace ChibitsLink.main.cs.converters;

public class BooleanToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (bool)(value ?? false) ? "VICTORIA" : "DERROTA";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BooleanToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (bool)(value ?? false) ? Color.FromArgb("#00B894") : Color.FromArgb("#FF6584");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BooleanToReadyTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (bool)(value ?? false) ? "¡ESTOY LISTO!" : "MARCAR COMO LISTO";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
