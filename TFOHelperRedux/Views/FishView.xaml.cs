using System.Windows.Controls;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Views;

public partial class FishView : UserControl
{
    public FishView()
    {
        InitializeComponent();
        DataContext = new FishViewModel();
    }
}