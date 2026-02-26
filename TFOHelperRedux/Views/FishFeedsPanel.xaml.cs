using System.Windows;
using System.Windows.Controls;
using TFOHelperRedux.Models;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Views
{
    public partial class FishFeedsPanel : UserControl
    {
        public static readonly DependencyProperty CatchPointProperty = DependencyProperty.Register(
            nameof(CatchPoint), typeof(CatchPointModel), typeof(FishFeedsPanel),
            new PropertyMetadata(null, OnCatchPointChanged));

        public CatchPointModel? CatchPoint
        {
            get => (CatchPointModel?)GetValue(CatchPointProperty);
            set => SetValue(CatchPointProperty, value);
        }

        /// <summary>
        /// Возвращает ID выбранных рецептов
        /// </summary>
        public int[] SelectedRecipeIds
        {
            get
            {
                // В режиме точки лова возвращаем RecipeIDs из точки
                if (CatchPoint != null)
                    return CatchPoint.RecipeIDs ?? Array.Empty<int>();

                return Array.Empty<int>();
            }
        }

        /// <summary>
        /// Возвращает ID выбранных прикормок
        /// </summary>
        public int[] SelectedFeedIds
        {
            get
            {
                // В режиме точки лова возвращаем FeedIDs из точки
                if (CatchPoint != null)
                    return CatchPoint.FeedIDs ?? Array.Empty<int>();

                return Array.Empty<int>();
            }
        }

        public FishFeedsPanel()
        {
            InitializeComponent();
        }

        private static void OnCatchPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FishFeedsPanel panel && panel.DataContext is FishFeedsViewModel vm)
            {
                vm.SetCatchPoint(e.NewValue as CatchPointModel);
            }
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is FishFeedsViewModel vm)
            {
                vm.SearchText = tbSearch.Text;
            }
        }

        private void Feed_Checked(object sender, RoutedEventArgs e)
        {
            if (CatchPoint != null && sender is CheckBox cb && cb.Tag is int feedId)
            {
                var list = CatchPoint.FeedIDs?.ToList() ?? new List<int>();
                if (!list.Contains(feedId))
                    list.Add(feedId);
                CatchPoint.FeedIDs = list.ToArray();
                CatchPoint.OnPropertyChanged(nameof(CatchPoint.FeedIDs));
            }
        }

        private void Feed_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CatchPoint != null && sender is CheckBox cb && cb.Tag is int feedId)
            {
                var list = CatchPoint.FeedIDs?.ToList() ?? new List<int>();
                if (list.Contains(feedId))
                    list.Remove(feedId);
                CatchPoint.FeedIDs = list.ToArray();
                CatchPoint.OnPropertyChanged(nameof(CatchPoint.FeedIDs));
            }
        }

        private void Recipe_Checked(object sender, RoutedEventArgs e)
        {
            if (CatchPoint != null && sender is CheckBox cb && cb.Tag is int recipeId)
            {
                var list = CatchPoint.RecipeIDs?.ToList() ?? new List<int>();
                if (!list.Contains(recipeId))
                    list.Add(recipeId);
                CatchPoint.RecipeIDs = list.ToArray();
                CatchPoint.OnPropertyChanged(nameof(CatchPoint.RecipeIDs));
            }
        }

        private void Recipe_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CatchPoint != null && sender is CheckBox cb && cb.Tag is int recipeId)
            {
                var list = CatchPoint.RecipeIDs?.ToList() ?? new List<int>();
                if (list.Contains(recipeId))
                    list.Remove(recipeId);
                CatchPoint.RecipeIDs = list.ToArray();
                CatchPoint.OnPropertyChanged(nameof(CatchPoint.RecipeIDs));
            }
        }
    }
}
