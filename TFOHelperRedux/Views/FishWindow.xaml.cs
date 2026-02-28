using System.Windows;
using TFOHelperRedux.Services;

namespace TFOHelperRedux.Views
{
    public partial class FishWindow : Window
    {
        public FishWindow()
        {
            InitializeComponent();
            Loaded += FishWindow_Loaded;
        }

        private async void FishWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Автообновление при запуске
            var updateService = new UpdateService("https://raw.githubusercontent.com/skippoff/TFOHelperRedux/master/update.xml");
            await updateService.CheckAndUpdateAsync();
        }
    }
}