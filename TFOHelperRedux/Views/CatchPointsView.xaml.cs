using System.Windows;
using System.Windows.Controls;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.State;

namespace TFOHelperRedux.Views;

public partial class CatchPointsView : UserControl
{
    public CatchPointsView()
    {
        InitializeComponent();
        // DataContext устанавливается из родителя (FishDetailsPanel → FishViewModel → CatchPointsVM)
    }
}
