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
                // Вызываем событие добавления в рецепт
                DataStore.AddToRecipe?.Invoke(item);
            }
        }
    }
}
