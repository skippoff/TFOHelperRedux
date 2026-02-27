using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace TFOHelperRedux.Converters
{
    public class StringToImageSourceConverter : IValueConverter
    {
        private static readonly ConcurrentDictionary<string, BitmapImage> _cache = new();

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return null;

            var fullPath = Path.GetFullPath(path);

            // Проверяем кэш
            if (_cache.TryGetValue(fullPath, out var cachedImage))
                return cachedImage;

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(fullPath);
                image.CacheOption = BitmapCacheOption.OnLoad;
                
                // Уменьшаем размер для производительности
                image.DecodePixelWidth = 128;
                image.DecodePixelHeight = 128;
                
                image.EndInit();
                image.Freeze();

                // Добавляем в кэш
                _cache[fullPath] = image;

                return image;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}