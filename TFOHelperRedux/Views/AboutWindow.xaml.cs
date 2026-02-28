using System.Reflection;
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

            string version = Assembly.GetExecutingAssembly()
                .GetName().Version?.ToString(3) ?? "unknown";
            VersionTextBlock.Text = $"Версия {version}";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
