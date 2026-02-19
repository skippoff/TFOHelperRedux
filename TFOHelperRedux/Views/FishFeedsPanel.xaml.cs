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
