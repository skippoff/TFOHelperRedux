using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;

namespace TFOHelperRedux.Converters
{
    public class RecipeCompositionConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not BaitRecipeModel recipe)
                return null;

            var items = new List<string>();

            // Прикормка
            foreach (var id in recipe.FeedIDs ?? Array.Empty<int>())
            {
                var feed = DataStore.Feeds.FirstOrDefault(f => f.ID == id);
                items.Add($"Прикормка: {feed?.Name ?? id.ToString()}");
            }

            // Наживка
            foreach (var id in recipe.LureIDs ?? Array.Empty<int>())
            {
                var lure = DataStore.Lures.FirstOrDefault(l => l.ID == id);
                items.Add($"Наживка: {lure?.Name ?? id.ToString()}");
            }

            // Дип
            foreach (var id in recipe.DipIDs ?? Array.Empty<int>())
            {
                var dip = DataStore.Dips.FirstOrDefault(d => d.ID == id);
                items.Add($"Дип: {dip?.Name ?? id.ToString()}");
            }

            // Компоненты
            if (DataStore.FeedComponents != null)
            {
                foreach (var id in recipe.ComponentIDs ?? Array.Empty<int>())
                {
                    var comp = DataStore.FeedComponents.FirstOrDefault(c => c.ID == id);
                    items.Add($"Компонент: {comp?.Name ?? id.ToString()}");
                }
            }

            return items;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}