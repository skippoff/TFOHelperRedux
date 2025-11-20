using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Views;

public partial class CatchPointsView : UserControl
{
    private MapPreviewWindow? _mapWindow;
    private readonly DispatcherTimer _clickTimer;
    private CatchPointModel? _pendingPoint;   // точка, ожидающая «одиночного» действия

    public CatchPointsView()
    {
        InitializeComponent();

        _clickTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(220)
        };
        _clickTimer.Tick += (s, e) =>
        {
            _clickTimer.Stop();
            if (_pendingPoint == null) return;

            // показываем карту (одинарный клик)
            var map = Services.DataStore.Maps.FirstOrDefault(m => m.ID == _pendingPoint.MapID);
            if (map != null)
            {
                if (_mapWindow == null || !_mapWindow.IsLoaded)
                {
                    _mapWindow = new Views.MapPreviewWindow(map, _pendingPoint);
                    _mapWindow.Show();
                }
                else
                {
                    _mapWindow.UpdatePoint(map, _pendingPoint);
                    if (!_mapWindow.IsVisible)
                        _mapWindow.Show();

                    _mapWindow.Activate();
                }
            }
            _pendingPoint = null;
        };
    }
    private void CatchPoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border border || border.DataContext is not CatchPointModel point)
            return;

        // Если клик был по кнопке внутри карточки – ничего не делаем,
        // кнопка сама обработает команду
        if (e.OriginalSource is DependencyObject dep)
        {
            var current = dep;
            while (current != null && current != border)
            {
                if (current is Button)
                    return;

                current = VisualTreeHelper.GetParent(current);
            }
        }

        // 🟢 Один клик → показать карту
        if (e.ClickCount == 1)
        {
            _pendingPoint = point;
            _clickTimer.Stop();
            _clickTimer.Start();
            var map = TFOHelperRedux.Services.DataStore.Maps
                .FirstOrDefault(m => m.ID == point.MapID);

            if (map == null)
            {
                MessageBox.Show("Карта не найдена для этой точки.");
                return;
            }

            if (_mapWindow == null || !_mapWindow.IsLoaded)
            {
                _mapWindow = new TFOHelperRedux.Views.MapPreviewWindow(map, point);
                _mapWindow.Show();
            }
            else
            {
                _mapWindow.UpdatePoint(map, point);
                if (!_mapWindow.IsVisible)
                    _mapWindow.Show();

                _mapWindow.Activate();
            }
        }
        // 🔵 Двойной клик → редактирование точки
        else if (e.ClickCount == 2)
        {
            var wnd = new TFOHelperRedux.Views.EditCatchPointWindow(point);
            if (wnd.ShowDialog() == true &&
                DataContext is TFOHelperRedux.ViewModels.CatchPointsViewModel vm)
            {
                var fish = TFOHelperRedux.Services.DataStore.SelectedFish ?? vm.CurrentFish;
                vm.RefreshFilteredPoints(fish);
            }

            // Если окно карты открыто — обновим маркер
            if (_mapWindow is { IsLoaded: true })
            {
                var map = TFOHelperRedux.Services.DataStore.Maps
                    .FirstOrDefault(m => m.ID == point.MapID);
                if (map != null)
                    _mapWindow.UpdatePoint(map, point);
            }
        }

        e.Handled = true;
    }

}
