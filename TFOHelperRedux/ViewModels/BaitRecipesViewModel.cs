using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;

namespace TFOHelperRedux.ViewModels
{
    public class BaitRecipesViewModel : BaseViewModel
    {
        private readonly BaitRecipeService _recipeService;

        // Теперь это отдельная коллекция для отображения (только не скрытые рецепты)
        public ObservableCollection<BaitRecipeModel> Recipes { get; } = new();

        private BaitRecipeModel? _currentRecipe;
        public BaitRecipeModel? CurrentRecipe
        {
            get => _currentRecipe;
            set
            {
                _currentRecipe = value;
                RecipeName = value?.Name ?? "";
                UpdatePreviewList();
                OnPropertyChanged();
            }
        }

        private string _recipeName = "";
        public string RecipeName
        {
            get => _recipeName;
            set
            {
                _recipeName = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> PreviewItems { get; } = new();

        public ICommand SaveRecipeCmd { get; }
        public ICommand NewRecipeCmd { get; }
        public ICommand DeleteRecipeCmd { get; }
        public ICommand ClearRecipeCmd { get; }
        public ICommand AttachRecipeToFishCmd { get; }
        public ICommand DetachRecipeFromFishCmd { get; }

        public BaitRecipesViewModel()
        {
            _recipeService = new BaitRecipeService();

            // гарантируем, что главная коллекция существует
            if (DataStore.BaitRecipes == null)
                DataStore.BaitRecipes = new ObservableCollection<BaitRecipeModel>();

            _recipeService.NormalizeRecipeIds(DataStore.BaitRecipes);
            RebuildRecipesList();

            SaveRecipeCmd = new RelayCommand(SaveRecipe);
            NewRecipeCmd = new RelayCommand(NewRecipe);
            DeleteRecipeCmd = new RelayCommand(DeleteRecipe);
            ClearRecipeCmd = new RelayCommand(ClearRecipe);
            AttachRecipeToFishCmd = new RelayCommand(AttachRecipeToFish);
            DetachRecipeFromFishCmd = new RelayCommand(DetachRecipeFromFish);

            // Связываем двойной клик из левой панели
            DataStore.AddToRecipe = AddToCurrentRecipe;
        }

        // Добавление элемента (из левой панели) в текущий рецепт
        public void AddToCurrentRecipe(IItemModel item)
        {
            if (item == null) return;
            if (CurrentRecipe == null)
                CurrentRecipe = _recipeService.CreateNewRecipe();

            _recipeService.AddItemToRecipe(CurrentRecipe, item);
            UpdatePreviewList();
        }

        private void UpdatePreviewList()
        {
            PreviewItems.Clear();
            if (CurrentRecipe == null) return;

            foreach (var id in CurrentRecipe.FeedIDs)
                PreviewItems.Add($"Прикормка: {DataStore.Feeds.FirstOrDefault(f => f.ID == id)?.Name ?? id.ToString()}");

            foreach (var id in CurrentRecipe.LureIDs)
                PreviewItems.Add($"Наживка: {DataStore.Lures.FirstOrDefault(f => f.ID == id)?.Name ?? id.ToString()}");

            foreach (var id in CurrentRecipe.DipIDs)
                PreviewItems.Add($"Дип: {DataStore.Dips.FirstOrDefault(d => d.ID == id)?.Name ?? id.ToString()}");

            if (DataStore.FeedComponents != null)
            {
                foreach (var id in CurrentRecipe.ComponentIDs)
                    PreviewItems.Add($"Компонент: {DataStore.FeedComponents.FirstOrDefault(c => c.ID == id)?.Name ?? id.ToString()}");
            }

            OnPropertyChanged(nameof(PreviewItems));
        }

        // 🔹 список для отображения: только не скрытые рецепты
        private void RebuildRecipesList()
        {
            Recipes.Clear();

            if (DataStore.BaitRecipes == null)
                DataStore.BaitRecipes = new ObservableCollection<BaitRecipeModel>();

            foreach (var r in DataStore.BaitRecipes.Where(r => !r.IsHidden))
                Recipes.Add(r);
        }

        private void SaveRecipe()
        {
            if (CurrentRecipe == null || string.IsNullOrWhiteSpace(RecipeName))
            {
                MessageBox.Show("Введите название рецепта.", "Сохранение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // обновляем поля текущего рецепта
            CurrentRecipe.Name = RecipeName;

            _recipeService.SaveRecipe(CurrentRecipe, DataStore.BaitRecipes);
            RebuildRecipesList();

            MessageBox.Show("Рецепт сохранён.", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NewRecipe()
        {
            CurrentRecipe = _recipeService.CreateNewRecipe();
            RecipeName = "";
            PreviewItems.Clear();
        }

        private void ClearRecipe()
        {
            if (CurrentRecipe == null) return;

            if (MessageBox.Show("Очистить текущий рецепт?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _recipeService.ClearRecipe(CurrentRecipe);
                UpdatePreviewList();
            }
        }

        private void DeleteRecipe()
        {
            if (CurrentRecipe == null) return;

            var result = MessageBox.Show(
                $"Удалить рецепт '{CurrentRecipe.Name}' только из крафтового списка?\n" +
                "Привязки к рыбе сохранятся.",
                "Удаление",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            _recipeService.HideRecipe(CurrentRecipe);
            RebuildRecipesList();
            NewRecipe();
        }

        private void AttachRecipeToFish(object? parameter)
        {
            if (parameter is not BaitRecipeModel recipe)
                return;

            var result = _recipeService.AttachRecipeToFish(recipe, DataStore.SelectedFish);
            result.ShowMessageBox();
        }

        private void DetachRecipeFromFish(object? parameter)
        {
            if (parameter is not BaitRecipeModel recipe)
                return;

            var result = _recipeService.DetachRecipeFromFish(recipe, DataStore.SelectedFish);
            result.ShowMessageBox();
        }
    }
}
