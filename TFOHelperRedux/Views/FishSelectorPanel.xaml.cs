using System.Windows.Controls;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Views
{
    public partial class FishSelectorPanel : UserControl
    {
        public FishSelectorPanel()
        {
            InitializeComponent();
        }

        private void FishList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is not FishViewModel vm) return;

            var fish = vm.SelectedFish;
            var recipe = vm.BaitRecipesVM.CurrentRecipe;

            if (fish == null || recipe == null)
                return;

            // Если эта рыба ещё не добавлена к рецепту
            if (!recipe.FishIDs.Contains(fish.ID))
            {
                recipe.FishIDs = recipe.FishIDs.Append(fish.ID).ToArray();
                vm.BaitRecipesVM.SaveRecipeCmd.Execute(null);
            }
        }

    }
}