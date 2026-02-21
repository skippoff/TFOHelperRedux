using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace TFOHelperRedux.Views.Controls
{
    public partial class BiteIntensityChart : UserControl
    {
        // 🟢 Поля для перетаскивания графика
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private double _scrollStartOffset;

        // 🟢 DependencyProperty для массива интенсивности клёва
        public static readonly DependencyProperty BiteIntensityProperty =
            DependencyProperty.Register(
                nameof(BiteIntensity),
                typeof(int[]),
                typeof(BiteIntensityChart),
                new PropertyMetadata(null));

        // 🟢 Событие для изменения значения интенсивности
        public event EventHandler<HourChangedEventArgs>? HourChanged;

        public int[]? BiteIntensity
        {
            get => (int[]?)GetValue(BiteIntensityProperty);
            set => SetValue(BiteIntensityProperty, value);
        }

        public BiteIntensityChart()
        {
            InitializeComponent();
        }

        // 🟢 Обработчик кликов по столбцам графика
        private void BiteBar_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border b && b.Tag is int hour && BiteIntensity != null)
            {
                // создаём копию массива, чтобы триггерить PropertyChanged
                var arr = BiteIntensity.ToArray();
                arr[hour] = (arr[hour] + 1) % 11; // увеличиваем уровень (0..10)
                BiteIntensity = arr;

                // вызываем событие для внешней обработки
                HourChanged?.Invoke(this, new HourChangedEventArgs(hour, arr[hour]));
            }
        }

        // 🟢 Обработчики перетаскивания графика
        private void Chart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _dragStartPoint = e.GetPosition(ChartScrollViewer);
                _scrollStartOffset = ChartScrollViewer.HorizontalOffset;
                ChartScrollViewer.CaptureMouse();
                ChartScrollViewer.Cursor = Cursors.SizeWE;
                e.Handled = true;
            }
        }

        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && ChartScrollViewer.IsMouseCaptured)
            {
                var currentPoint = e.GetPosition(ChartScrollViewer);
                var delta = _dragStartPoint.X - currentPoint.X;
                ChartScrollViewer.ScrollToHorizontalOffset(_scrollStartOffset + delta);
            }
        }

        private void Chart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            StopDragging();
        }

        private void Chart_MouseLeave(object sender, MouseEventArgs e)
        {
            StopDragging();
        }

        private void Chart_LostMouseCapture(object? sender, MouseEventArgs e)
        {
            StopDragging();
        }

        private void StopDragging()
        {
            if (_isDragging)
            {
                _isDragging = false;
                if (ChartScrollViewer.IsMouseCaptured)
                    ChartScrollViewer.ReleaseMouseCapture();
                ChartScrollViewer.Cursor = Cursors.Hand;
            }
        }
    }

    // 🟢 Аргументы события изменения часа
    public class HourChangedEventArgs : EventArgs
    {
        public int Hour { get; }
        public int NewValue { get; }

        public HourChangedEventArgs(int hour, int newValue)
        {
            Hour = hour;
            NewValue = newValue;
        }
    }
}
