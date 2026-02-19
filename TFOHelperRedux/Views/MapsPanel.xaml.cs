using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Views
{
    public partial class MapsPanel : UserControl
    {
        public MapsPanel()
        {
            InitializeComponent();
        }
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scroll)
            {
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset - e.Delta / 3.0);
                e.Handled = true;
            }
        }
        private void EditMapFishes_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            if (DataContext is FishViewModel vm)
            {
                vm.EditMapFishesCmd.Execute(null);
            }
#endif
        }

    }
}