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
                if (DataContext is FishFeedsViewModel vm)
                {
                    return vm.Recipes.Where(r => r.IsSelected).Select(r => r.ID).ToArray();
                }
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
                if (DataContext is FishFeedsViewModel vm)
                {
                    return vm.Feeds.Where(f => f.IsSelected).Select(f => f.ID).ToArray();
                }
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
    }
}
