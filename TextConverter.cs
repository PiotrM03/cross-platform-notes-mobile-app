using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace NotesApp;

public class TextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text && parameter is string param && int.TryParse(param, out int maxLength))
        {
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
