using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Converters
{
    public class RecipeRankToStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RecipeRank rank)
            {
                string key = rank switch
                {
                    RecipeRank.Top => "RecipeBorderTop",
                    RecipeRank.Experimental => "RecipeBorderExperimental",
                    RecipeRank.Deprecated => "RecipeBorderDeprecated",
                    _ => "RecipeBorderNormal"
                };

                return Application.Current.FindResource(key);
            }

            return Application.Current.FindResource("RecipeBorderNormal");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}