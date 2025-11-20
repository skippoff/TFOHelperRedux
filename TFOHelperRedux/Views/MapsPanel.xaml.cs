using System.Windows.Controls;
using System.Windows.Input;
using TFOHelperRedux.ViewModels;
using System.Windows;

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
            // DataContext здесь – FishViewModel
            if (DataContext is not FishViewModel vm)
                return;

            var map = vm.SelectedMap;
            if (map == null)
            {
                MessageBox.Show("Сначала выбери локацию слева.",
                    "Редактирование локации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var win = new EditMapFishesWindow(map)
            {
                Owner = Window.GetWindow(this)
            };
            win.ShowDialog();

            // После изменений обновляем список рыб по карте
            var current = vm.SelectedMap;
            vm.SelectedMap = null;
            vm.SelectedMap = current;
#endif
        }

    }
}