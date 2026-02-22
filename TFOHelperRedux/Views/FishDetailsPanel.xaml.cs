using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.State;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Views
{
    public partial class FishDetailsPanel : UserControl
    {
        private bool _isDragging = false;
        private const double MaxBiteValue = 10.0;

        public FishDetailsPanel()
        {
            InitializeComponent();
            Loaded += FishDetailsPanel_Loaded;
        }

        private void FishDetailsPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is FishViewModel vm)
            {
                vm.PropertyChanged += ViewModel_PropertyChanged;
                UpdateVisibility(vm.SelectedFish);
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FishViewModel.SelectedFish))
            {
                UpdateVisibility((sender as FishViewModel)?.SelectedFish);
            }
        }

        private void UpdateVisibility(object? selectedFish)
        {
            var isVisible = selectedFish != null;
            FishDetailsContent.Visibility = isVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            FishPlaceholder.Visibility = isVisible ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        // 🟢 Обработчики мыши для графика
        private void BiteChart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                ((UIElement)sender).CaptureMouse();
                UpdateBarFromMouse(sender, e.GetPosition((IInputElement)sender));
            }
        }

        private void BiteChart_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
                UpdateBarFromMouse(sender, e.GetPosition((IInputElement)sender));
        }

        private void BiteChart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ((UIElement)sender).ReleaseMouseCapture();
        }

        private void BiteChart_MouseLeave(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            ((UIElement)sender).ReleaseMouseCapture();
        }

        private void UpdateBarFromMouse(object sender, Point pos)
        {
            if (DataContext is not FishViewModel vm || vm.SelectedFish == null) return;

            var chart = (LiveChartsCore.SkiaSharpView.WPF.CartesianChart)sender;
            var series = vm.BiteChartSeries?.FirstOrDefault() as LiveChartsCore.SkiaSharpView.ColumnSeries<int>;
            if (series?.Values == null) return;

            // Переводим пиксели в координаты графика
            var scaledPoint = chart.ScalePixelsToData(new LiveChartsCore.Drawing.LvcPointD(pos.X, pos.Y));

            int index = (int)Math.Round(scaledPoint.X);
            double value = Math.Clamp(scaledPoint.Y, 0, MaxBiteValue);

            if (index >= 0 && index < series.Values.Count)
            {
                // Обновляем значение в серии
                var values = series.Values.ToArray();
                values[index] = (int)Math.Round(value);
                
                // Обновляем модель
                var arr = vm.SelectedFish.BiteIntensity.ToArray();
                arr[index] = (int)Math.Round(value);
                vm.SelectedFish.BiteIntensity = arr;

                // Обновляем серию для LiveCharts
                series.Values = vm.SelectedFish.BiteIntensity;

                vm.RefreshSelectedFish();
                vm.OnPropertyChanged(nameof(vm.BiteDescription));
                DataService.SaveFishes(DataStore.Fishes);
            }
        }
    }
}