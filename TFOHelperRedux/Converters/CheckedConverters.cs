using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using TFOHelperRedux.Models;

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
    /// <summary>
    /// Конвертер для чекбоксов прикормок. Проверяет наличие ID в массиве.
    /// Для MultiBinding: первый параметр — CatchPoint, второй — ID прикормки
    /// </summary>
    public class FeedCheckedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return false;

            var catchPoint = values[0] as CatchPointModel;
            var feedIds = catchPoint?.FeedIDs;

            if (feedIds == null)
                return false;

            if (values[1] is int id)
                return feedIds.Contains(id);

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class DipCheckedConverter : FishCheckedConverter { }

    /// <summary>
    /// Конвертер для чекбоксов наживок. Проверяет наличие ID наживки в массиве ID точки лова.
    /// Для MultiBinding: первый параметр — массив LureIDs, второй — ID наживки
    /// </summary>
    public class LureCheckedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return false;

            var lureIds = values[0] as int[];
            if (lureIds == null)
                return false;

            if (values[1] is int lureId)
                return lureIds.Contains(lureId);

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // Возвращаем ID для обработки в событии Checked/Unchecked
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для чекбоксов лучших наживок. Проверяет наличие ID в BestLureIDs.
    /// Для MultiBinding: первый параметр — массив BestLureIDs, второй — ID наживки
    /// </summary>
    public class BestLureCheckedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return false;

            var bestLureIds = values[0] as int[];
            if (bestLureIds == null)
                return false;

            if (values[1] is int lureId)
                return bestLureIds.Contains(lureId);

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для чекбоксов рецептов. Проверяет наличие ID рецепта в массиве ID.
    /// Для MultiBinding: первый параметр — CatchPoint, второй — ID рецепта
    /// </summary>
    public class RecipeCheckedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return false;

            var catchPoint = values[0] as CatchPointModel;
            var recipeIds = catchPoint?.RecipeIDs;

            if (recipeIds == null)
                return false;

            if (values[1] is int id)
                return recipeIds.Contains(id);

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
