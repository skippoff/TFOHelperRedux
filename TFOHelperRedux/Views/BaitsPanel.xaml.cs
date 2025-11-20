using System.Windows.Controls;
using System.Windows.Input;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Views
{
    public partial class BaitsPanel : UserControl
    {
        public BaitsPanel()
        {
            InitializeComponent();
        }
        private void Ingredient_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox lb && lb.SelectedItem is IItemModel item)
            {
                // Наш VM – это FishViewModel
                if (DataContext is FishViewModel vm && vm.BaitsSubMode == "CraftLures")
                {
                    // Режим крафтовых наживок
                    DataStore.AddToCraftLure?.Invoke(item);
                }
                else
                {
                    // Обычные рецепты прикорма
                    DataStore.AddToRecipe?.Invoke(item);
                }
            }
        }
    }
}