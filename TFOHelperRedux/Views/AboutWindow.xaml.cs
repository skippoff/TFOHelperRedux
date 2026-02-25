using System.Windows;

namespace TFOHelperRedux.Views
{
    /// <summary>
    /// Логика взаимодействия для AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow(Window? owner = null)
        {
            InitializeComponent();
            
            if (owner != null)
            {
                Owner = owner;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
