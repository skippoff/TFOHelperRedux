using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace TFOHelperRedux.Converters
{
    public class FishCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ids = value as int[];
            if (ids == null || parameter == null)
                return false;

            if (int.TryParse(parameter.ToString(), out int id))
                return ids.Contains(id);

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return null;

            if (!int.TryParse(parameter.ToString(), out int id))
                return null;

            var isChecked = (bool)value;
            var current = value as int[] ?? Array.Empty<int>();

            // ⚠️ value сюда не передаётся — нужно будет обновить через SelectedFish вручную в VM
            return null;
        }
    }
    public class FeedCheckedConverter : FishCheckedConverter { }
    public class DipCheckedConverter : FishCheckedConverter { }

}
