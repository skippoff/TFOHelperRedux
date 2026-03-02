using System;
using System.Globalization;
using System.Windows.Data;

namespace TFOHelperRedux.Converters;

/// <summary>
/// Удаляет префикс из строки (всё до ": " включительно)
/// Например: "Особенности: трофейная" → "трофейная"
/// </summary>
public class StripPrefixConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string str)
            return value;

        var idx = str.IndexOf(": ", StringComparison.Ordinal);
        return idx >= 0 ? str.Substring(idx + 2) : str;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
