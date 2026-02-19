using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Views
{
    public partial class MapsPanel : UserControl
    {
        public MapsPanel()
        {
            InitializeComponent();
        }

        private void EditMapFishes_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            if (DataContext is FishViewModel vm)
            {
                vm.EditMapFishesCmd.Execute(null);
            }
#endif
        }
    }
}