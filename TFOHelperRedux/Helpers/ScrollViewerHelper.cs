using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TFOHelperRedux.Helpers
{
    /// <summary>
    /// Прикрепляемое свойство для включения прокрутки колёсиком мыши в ScrollViewer
    /// </summary>
    public static class ScrollViewerHelper
    {
        public static readonly DependencyProperty EnableMouseWheelScrollProperty =
            DependencyProperty.RegisterAttached(
                "EnableMouseWheelScroll",
                typeof(bool),
                typeof(ScrollViewerHelper),
                new PropertyMetadata(false, OnEnableMouseWheelScrollChanged));

        public static void SetEnableMouseWheelScroll(UIElement element, bool value)
            => element.SetValue(EnableMouseWheelScrollProperty, value);

        public static bool GetEnableMouseWheelScroll(UIElement element)
            => (bool)element.GetValue(EnableMouseWheelScrollProperty);

        private static void OnEnableMouseWheelScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                    element.PreviewMouseWheel += Element_PreviewMouseWheel;
                else
                    element.PreviewMouseWheel -= Element_PreviewMouseWheel;
            }
        }

        private static void Element_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer sv)
            {
                sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }
    }
}
