using System;
using System.Globalization;
using System.Windows.Data;

namespace TFOHelperRedux.Converters
{
    public class MapGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDlc)
                return isDlc ? "Локации DLC" : "Обычные локации";
            
            if (value is string s && bool.TryParse(s, out isDlc))
                return isDlc ? "Локации DLC" : "Обычные локации";

            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
