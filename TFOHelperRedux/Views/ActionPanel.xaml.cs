using System.Windows;
using System.Windows.Controls;
using TFOHelperRedux.Views;

namespace TFOHelperRedux.Views
{
    /// <summary>
    /// Логика взаимодействия для ActionPanel.xaml
    /// </summary>
    public partial class ActionPanel : UserControl
    {
        public ActionPanel()
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
