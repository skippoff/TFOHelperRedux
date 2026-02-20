using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TFOHelperRedux.Controls;

/// <summary>
/// Кастомный контрол графика интенсивности клёва с drag-to-set.
/// Тянешь мышкой вверх/вниз — значение столбца меняется (0-10).
/// </summary>
public class BiteChartControl : Control
{
    private const int HourCount = 24;
    private const int MaxValue = 10;
    private const double TopPadding = 18;
    private const double BottomPadding = 22;
    private const double LeftPadding = 28;
    private const double RightPadding = 8;

    private bool _isDragging;
    private int _lastSetHour = -1;

    #region Dependency Properties

    public static readonly DependencyProperty BiteIntensityProperty =
        DependencyProperty.Register(nameof(BiteIntensity), typeof(int[]), typeof(BiteChartControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public int[]? BiteIntensity
    {
        get => (int[]?)GetValue(BiteIntensityProperty);
        set => SetValue(BiteIntensityProperty, value);
    }

    public static readonly DependencyProperty BarBrushProperty =
        DependencyProperty.Register(nameof(BarBrush), typeof(Brush), typeof(BiteChartControl),
            new FrameworkPropertyMetadata(Brushes.MediumSeaGreen, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush BarBrush
    {
        get => (Brush)GetValue(BarBrushProperty);
        set => SetValue(BarBrushProperty, value);
    }

    public static readonly DependencyProperty BarHoverBrushProperty =
        DependencyProperty.Register(nameof(BarHoverBrush), typeof(Brush), typeof(BiteChartControl),
            new FrameworkPropertyMetadata(Brushes.LimeGreen, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush BarHoverBrush
    {
        get => (Brush)GetValue(BarHoverBrushProperty);
        set => SetValue(BarHoverBrushProperty, value);
    }

    public static readonly DependencyProperty GridLineBrushProperty =
        DependencyProperty.Register(nameof(GridLineBrush), typeof(Brush), typeof(BiteChartControl),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(40, 128, 128, 128)),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush GridLineBrush
    {
        get => (Brush)GetValue(GridLineBrushProperty);
        set => SetValue(GridLineBrushProperty, value);
    }

    public static readonly DependencyProperty LabelBrushProperty =
        DependencyProperty.Register(nameof(LabelBrush), typeof(Brush), typeof(BiteChartControl),
            new FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush LabelBrush
    {
        get => (Brush)GetValue(LabelBrushProperty);
        set => SetValue(LabelBrushProperty, value);
    }

    /// <summary>Событие при изменении значения (при каждом движении мыши)</summary>
    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(BiteChartControl));

    public event RoutedEventHandler ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    /// <summary>Событие при завершении редактирования (отпускание мыши — для сохранения)</summary>
    public static readonly RoutedEvent EditCompletedEvent =
        EventManager.RegisterRoutedEvent(nameof(EditCompleted), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(BiteChartControl));

    public event RoutedEventHandler EditCompleted
    {
        add => AddHandler(EditCompletedEvent, value);
        remove => RemoveHandler(EditCompletedEvent, value);
    }

    #endregion

    public BiteChartControl()
    {
        ClipToBounds = true;
        Cursor = Cursors.Hand;
    }

    #region Rendering

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        var data = BiteIntensity;
        if (data == null || data.Length < HourCount) return;

        double chartW = w - LeftPadding - RightPadding;
        double chartH = h - TopPadding - BottomPadding;
        double barW = chartW / HourCount;
        double barInner = Math.Max(barW - 4, 2);

        var gridPen = new Pen(GridLineBrush, 1);
        gridPen.Freeze();

        var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        // Горизонтальные линии сетки (0, 2, 4, 6, 8, 10)
        for (int v = 0; v <= MaxValue; v += 2)
        {
            double y = TopPadding + chartH - (chartH * v / MaxValue);
            dc.DrawLine(gridPen, new Point(LeftPadding, y), new Point(w - RightPadding, y));

            var label = new FormattedText(v.ToString(), CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, typeface, 10, LabelBrush, 1.0);
            dc.DrawText(label, new Point(LeftPadding - label.Width - 4, y - label.Height / 2));
        }

        // Столбцы
        var hoverPos = Mouse.GetPosition(this);
        int hoverHour = GetHourFromX(hoverPos.X);

        for (int i = 0; i < HourCount; i++)
        {
            int val = Math.Clamp(data[i], 0, MaxValue);
            double barH = chartH * val / MaxValue;
            double x = LeftPadding + i * barW + (barW - barInner) / 2;
            double y = TopPadding + chartH - barH;

            var brush = (i == hoverHour && IsMouseOver) ? BarHoverBrush : BarBrush;

            // Скруглённый столбец
            if (barH > 0)
            {
                double radius = Math.Min(3, barInner / 2);
                var rect = new Rect(x, y, barInner, barH);
                dc.DrawRoundedRectangle(brush, null, rect, radius, radius);
            }

            // Значение над столбцом
            if (val > 0)
            {
                var valText = new FormattedText(val.ToString(), CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, typeface, 10, LabelBrush, 1.0);
                dc.DrawText(valText, new Point(x + (barInner - valText.Width) / 2, y - valText.Height - 1));
            }

            // Подпись часа
            var hourText = new FormattedText(i.ToString(), CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, typeface, 10, LabelBrush, 1.0);
            dc.DrawText(hourText, new Point(
                x + (barInner - hourText.Width) / 2,
                TopPadding + chartH + 4));
        }
    }

    #endregion

    #region Mouse interaction (drag-to-set)

    private int GetHourFromX(double x)
    {
        double chartW = ActualWidth - LeftPadding - RightPadding;
        if (chartW <= 0) return -1;
        double barW = chartW / HourCount;
        int hour = (int)((x - LeftPadding) / barW);
        return (hour >= 0 && hour < HourCount) ? hour : -1;
    }

    private int GetValueFromY(double y)
    {
        double chartH = ActualHeight - TopPadding - BottomPadding;
        if (chartH <= 0) return 0;
        double ratio = 1.0 - (y - TopPadding) / chartH;
        return Math.Clamp((int)Math.Round(ratio * MaxValue), 0, MaxValue);
    }

    private void SetValueAtPosition(Point pos)
    {
        var data = BiteIntensity;
        if (data == null || data.Length < HourCount) return;

        int hour = GetHourFromX(pos.X);
        int val = GetValueFromY(pos.Y);
        if (hour < 0) return;

        if (data[hour] != val)
        {
            // Мутируем массив напрямую и уведомляем
            data[hour] = val;
            _lastSetHour = hour;
            InvalidateVisual();
            RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        _isDragging = true;
        _lastSetHour = -1;
        CaptureMouse();
        SetValueAtPosition(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
        {
            SetValueAtPosition(e.GetPosition(this));
        }
        else
        {
            // Перерисовка для hover-эффекта
            InvalidateVisual();
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (_isDragging)
        {
            _isDragging = false;
            _lastSetHour = -1;
            ReleaseMouseCapture();
            RaiseEvent(new RoutedEventArgs(EditCompletedEvent));
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (_isDragging)
        {
            _isDragging = false;
            _lastSetHour = -1;
            ReleaseMouseCapture();
            RaiseEvent(new RoutedEventArgs(EditCompletedEvent));
        }
        InvalidateVisual();
    }

    #endregion
}
