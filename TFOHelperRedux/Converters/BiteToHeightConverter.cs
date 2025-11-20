using System.Globalization;
using System.Windows.Data;

namespace TFOHelperRedux.Converters
{
    public class BiteToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int bites)
                return Math.Max(5, bites * 10);
            return 5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}