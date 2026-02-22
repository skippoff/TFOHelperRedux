using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TFOHelperRedux.Views.Controls
{
    public partial class BiteIntensityChart : UserControl
    {
        // 🟢 Поля для перетаскивания столбца
        private bool _isDraggingBar = false;
        private int _draggingIndex = -1;
        private Border? _draggingBar;
        private const double ChartHeight = 100.0;

        // 🟢 Поля для рисования графика
        private bool _isPainting = false;
        private const double BarWidth = 20.0;

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

        // 🟢 Обработчики перетаскивания столбца
        private void Bar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            _draggingBar = (Border)sender;
            if (_draggingBar.Tag is not int index) return;
            
            _draggingIndex = index;
            _isDraggingBar = true;

            _draggingBar.CaptureMouse();
            _draggingBar.Cursor = Cursors.SizeNS;
            e.Handled = true;
        }

        private void UpdateBarFromMousePosition(Point pos)
        {
            if (_draggingIndex < 0 || BiteIntensity == null) return;

            double rawValue = ChartHeight - pos.Y;
            double clamped = Math.Clamp(rawValue, 0, ChartHeight);
            double normalized = clamped / ChartHeight; // 0.0 — 1.0

            // Создаём копию массива для триггера PropertyChanged
            var arr = BiteIntensity.ToArray();
            arr[_draggingIndex] = (int)Math.Round(normalized * 10);
            BiteIntensity = arr;

            // Вызываем событие для внешней обработки
            HourChanged?.Invoke(this, new HourChangedEventArgs(_draggingIndex, arr[_draggingIndex]));
        }

        private void StopBarDrag()
        {
            if (_isDraggingBar && _draggingBar != null)
            {
                _draggingBar.ReleaseMouseCapture();
                _draggingBar = null;
                _draggingIndex = -1;
                _isDraggingBar = false;
            }
        }

        // 🟢 Обработчики рисования графика (фон)
        private void Chart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDraggingBar)
            {
                _isPainting = true;
                ChartBorder.CaptureMouse();
                ChartBorder.Cursor = Cursors.Pen;
                PaintAtPosition(e.GetPosition(ChartGrid));
                e.Handled = true;
            }
        }

        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            // Если перетаскиваем столбец — обновляем его высоту
            if (_isDraggingBar)
            {
                UpdateBarFromMousePosition(e.GetPosition(ChartGrid));
                return;
            }
            
            // Если рисуем — обновляем значения
            if (_isPainting)
                PaintAtPosition(e.GetPosition(ChartGrid));
        }

        private void Chart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            StopPainting();
            StopBarDrag();
        }

        private void Chart_MouseLeave(object sender, MouseEventArgs e)
        {
            StopPainting();
            StopBarDrag();
        }

        private void StopPainting()
        {
            if (_isPainting)
            {
                _isPainting = false;
                if (ChartBorder.IsMouseCaptured)
                    ChartBorder.ReleaseMouseCapture();
                ChartBorder.Cursor = Cursors.Arrow;
            }
        }

        private void PaintAtPosition(Point pos)
        {
            if (BiteIntensity == null) return;

            int index = (int)(pos.X / BarWidth);
            if (index < 0 || index >= BiteIntensity.Length) return;

            double chartBottom = ChartGrid.ActualHeight;
            double rawValue = chartBottom - pos.Y;
            double clamped = Math.Clamp(rawValue, 0, ChartHeight);
            double normalized = clamped / ChartHeight;
            int newValue = (int)Math.Round(normalized * 10);

            var arr = BiteIntensity.ToArray();
            arr[index] = newValue;
            BiteIntensity = arr;

            HourChanged?.Invoke(this, new HourChangedEventArgs(index, newValue));
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
