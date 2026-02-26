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
        private static readonly MouseWheelEventHandler _mouseWheelHandler = new MouseWheelEventHandler(Element_PreviewMouseWheel);
        
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
            if (d is ScrollViewer sv)
            {
                if ((bool)e.NewValue)
                    sv.AddHandler(UIElement.PreviewMouseWheelEvent, _mouseWheelHandler, true);
                else
                    sv.RemoveHandler(UIElement.PreviewMouseWheelEvent, _mouseWheelHandler);
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
