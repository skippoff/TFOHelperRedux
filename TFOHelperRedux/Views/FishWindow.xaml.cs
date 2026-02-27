using System.Windows;
using AutoUpdaterDotNET;

namespace TFOHelperRedux.Views
{
    public partial class FishWindow : Window
    {
        public FishWindow()
        {
            AutoUpdater.Start("https://raw.githubusercontent.com/skippoff/TFOHelperRedux/main/update.xml");
            InitializeComponent();
        }
    }
}