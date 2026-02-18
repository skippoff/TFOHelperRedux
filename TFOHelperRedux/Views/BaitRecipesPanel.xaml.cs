using System.Windows.Controls;

namespace TFOHelperRedux.Views
{
    public partial class BaitRecipesPanel : UserControl
    {
        public BaitRecipesPanel()
        {
            InitializeComponent();
            // DataContext наследуется от родителя (BaitsPanel → FishViewModel → BaitRecipesVM)
        }
    }
}
