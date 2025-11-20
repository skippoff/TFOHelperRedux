using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TFOHelperRedux.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool b)
                flag = b;

            if (Invert)
                flag = !flag;

            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
                return (v == Visibility.Visible) ^ Invert;
            return false;
        }
    }
}