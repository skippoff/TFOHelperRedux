using System.Globalization;
using System.Windows.Data;

namespace TFOHelperRedux.Converters
{
    // Возвращает True, если строка value равна параметру — для IsChecked у кнопок
    public class EqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
