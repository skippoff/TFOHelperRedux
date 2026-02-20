using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Business;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.UI;

namespace TFOHelperRedux.ViewModels
{
    public class BaitRecipesViewModel : BaseViewModel
    {
        private readonly BaitRecipeService _recipeService;
        private readonly IUIService _uiService;

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

        public BaitRecipesViewModel(IUIService uiService)
        {
            _uiService = uiService;
            _recipeService = new BaitRecipeService();

            // гарантируем, что главная коллекция существует
            // свойство BaitRecipes само создаёт коллекцию при необходимости
            _recipeService.NormalizeRecipeIds(DataStore.BaitRecipes);
            RebuildRecipesList();

            // Подписка на изменения DataStore.BaitRecipes для автоматического обновления
            DataStore.BaitRecipes.CollectionChanged += (s, e) =>
            {
                // Перестраиваем список только при добавлении/удалении элементов
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                    e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                    e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    RebuildRecipesList();
                }
            };

            SaveRecipeCmd = new RelayCommand(SaveRecipe);
            NewRecipeCmd = new RelayCommand(NewRecipe);
            DeleteRecipeCmd = new RelayCommand(DeleteRecipe);
            ClearRecipeCmd = new RelayCommand(ClearRecipe);

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

            // свойство BaitRecipes само создаёт коллекцию при необходимости
            foreach (var r in DataStore.BaitRecipes.Where(r => !r.IsHidden))
                Recipes.Add(r);
        }

        private void SaveRecipe()
        {
            if (CurrentRecipe == null || string.IsNullOrWhiteSpace(RecipeName))
            {
                _uiService.ShowWarning("Введите название рецепта.", "Сохранение");
                return;
            }

            // обновляем поля текущего рецепта
            CurrentRecipe.Name = RecipeName;

            _recipeService.SaveRecipe(CurrentRecipe, DataStore.BaitRecipes);
            // RebuildRecipesList() не нужен — DataStore.BaitRecipes уже обновлён,
            // и FishFeedsViewModel получит CollectionChanged

            _uiService.ShowInfo("Рецепт сохранён.", "Успех");
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

            if (_uiService.ShowConfirm("Очистить текущий рецепт?", "Подтверждение"))
            {
                _recipeService.ClearRecipe(CurrentRecipe);
                UpdatePreviewList();
            }
        }

        private void DeleteRecipe()
        {
            if (CurrentRecipe == null) return;

            var result = _uiService.ShowMessageBox(
                $"Удалить рецепт '{CurrentRecipe.Name}' только из крафтового списка?\n" +
                "Привязки к рыбе сохранятся.",
                "Удаление",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            _recipeService.HideRecipe(CurrentRecipe);
            // RebuildRecipesList() вызывается автоматически через подписку на DataStore.BaitRecipes.CollectionChanged
            NewRecipe();
        }
    }
}
