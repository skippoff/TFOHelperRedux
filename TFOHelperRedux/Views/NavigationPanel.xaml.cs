using System.Windows;
using System.Windows.Controls;

namespace TFOHelperRedux.Views
{
    /// <summary>
    /// Логика взаимодействия для NavigationPanel.xaml
    /// </summary>
    public partial class NavigationPanel : UserControl
    {
        public NavigationPanel()
        {
            InitializeComponent();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow(Window.GetWindow(this));
            aboutWindow.ShowDialog();
        }
    }
}
