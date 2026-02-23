using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Views
{
    public partial class MapsPanel : UserControl
    {
        private double _scrollOffset = 0;
        private bool _isUserScrolling;
        private ScrollViewer? _scrollViewer;

        public MapsPanel()
        {
            InitializeComponent();
            Loaded += MapsPanel_Loaded;
        }

        private void MapsPanel_Loaded(object sender, RoutedEventArgs e)
        {
            _scrollViewer = GetScrollViewer(MapsListBox);
        }

        private void MapsScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Сохраняем позицию только если скроллит пользователь
            if (e.ViewportHeightChange == 0 && e.ViewportWidthChange == 0)
            {
                _scrollOffset = _scrollViewer?.VerticalOffset ?? 0;
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

        private void MapsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Восстанавливаем позицию скролла после выбора карты
            if (_scrollViewer != null && _scrollOffset > 0)
            {
                // Небольшая задержка для применения фильтра
                Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background,
                    new System.Action(() =>
                    {
                        _scrollViewer?.ScrollToVerticalOffset(_scrollOffset);
                    }));
            }
        }

        private ScrollViewer? GetScrollViewer(DependencyObject obj)
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);
                if (child is ScrollViewer sv)
                    return sv;

                var result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}