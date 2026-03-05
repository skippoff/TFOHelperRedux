using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Business;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.UI;
using TFOHelperRedux.Views;

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
        public ICommand RestoreFromBackupCmd { get; }
        public ICommand SaveCopyCmd { get; }

        private readonly BaitRecipesBackupService _backupService;

        public BaitRecipesViewModel(IUIService uiService)
        {
            _uiService = uiService;
            _recipeService = new BaitRecipeService();
            _backupService = new BaitRecipesBackupService();

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
            RestoreFromBackupCmd = new RelayCommand(RestoreFromBackup);
            SaveCopyCmd = new RelayCommand(SaveCopy);

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
            // Сохраняем текущий выбранный рецепт (по ID)
            var currentRecipeId = CurrentRecipe?.ID;
            
            Recipes.Clear();

            // свойство BaitRecipes само создаёт коллекцию при необходимости
            foreach (var r in DataStore.BaitRecipes.Where(r => !r.IsHidden))
                Recipes.Add(r);
            
            // Восстанавливаем CurrentRecipe, если он существует в коллекции
            if (currentRecipeId.HasValue)
            {
                var restoredRecipe = DataStore.BaitRecipes.FirstOrDefault(r => r.ID == currentRecipeId.Value);
                if (restoredRecipe != null && !restoredRecipe.IsHidden)
                {
                    // Не вызываем setter напрямую, чтобы избежать рекурсии
                    _currentRecipe = restoredRecipe;
                    RecipeName = restoredRecipe.Name;
                    UpdatePreviewList();
                    OnPropertyChanged(nameof(CurrentRecipe));
                }
            }
        }

        private void SaveRecipe()
        {
            try
            {
                if (CurrentRecipe == null)
                {
                    _uiService.ShowWarning("Текущий рецепт не выбран.", "Сохранение");
                    return;
                }

                if (string.IsNullOrWhiteSpace(RecipeName))
                {
                    _uiService.ShowWarning("Введите название рецепта.", "Сохранение");
                    return;
                }

                // Нормализуем рецепт перед сохранением (защита от null значений)
                CurrentRecipe.Normalize();

                // обновляем поля текущего рецепта
                CurrentRecipe.Name = RecipeName;
                // Rank уже обновляется через ComboBox (TwoWay binding)

                _recipeService.SaveRecipe(CurrentRecipe, DataStore.BaitRecipes);

                // Уведомляем UI об изменении свойств рецепта для перерисовки рамки
                if (CurrentRecipe != null)
                {
                    CurrentRecipe.NotifyPropertyChanged(nameof(CurrentRecipe.Name));
                    CurrentRecipe.NotifyPropertyChanged(nameof(CurrentRecipe.Rank));
                    CurrentRecipe.NotifyPropertyChanged(nameof(CurrentRecipe.DateEdited));
                }

                _uiService.ShowInfo("Рецепт сохранён.", "Успех");
            }
            catch (Exception ex)
            {
                _uiService?.ShowError($"Ошибка сохранения рецепта: {ex.Message}\n\n{ex.StackTrace}", "Ошибка");
                throw;
            }
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
                $"Удалить рецепт '{CurrentRecipe.Name}'?\n" +
                "Привязки к рыбе сохранятся, но рецепт исчезнет из списка крафтовых.",
                "Удаление",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            _recipeService.DeleteRecipe(CurrentRecipe);
            // Явно перестраиваем список, т.к. удаление вызывает CollectionChanged
            RebuildRecipesList();
            NewRecipe();
        }

        private void RestoreFromBackup()
        {
            var backups = _backupService.GetAvailableBackups();
            
            if (backups.Count == 0)
            {
                _uiService.ShowInfo("Бэкапы не найдены.", "Восстановление");
                return;
            }

            // Создаём окно выбора бэкапа
            var window = new BaitRecipesBackupWindow(backups);
            window.Owner = Application.Current.MainWindow;
            
            if (window.ShowDialog() == true && window.SelectedBackup != null)
            {
                var result = _uiService.ShowMessageBox(
                    $"Восстановить рецепты из бэкапа от {window.SelectedBackup.DisplayName}?\n\n" +
                    "Все текущие рецепты будут заменены.",
                    "Восстановление из бэкапа",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var recipes = _backupService.LoadFromBackup(window.SelectedBackup.FilePath);
                    
                    // Очищаем текущую коллекцию и добавляем загруженные рецепты
                    DataStore.BaitRecipes.Clear();
                    foreach (var recipe in recipes)
                    {
                        DataStore.BaitRecipes.Add(recipe);
                    }

                    RebuildRecipesList();
                    _uiService.ShowInfo("Рецепты восстановлены из бэкапа.", "Успех");
                }
            }
        }

        private void SaveCopy()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON файл|*.json",
                Title = "Сохранить копию рецептов",
                FileName = $"baitrecipes_backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _backupService.SaveCopy(DataStore.BaitRecipes, dialog.FileName);
                    _uiService.ShowInfo($"Копия рецептов сохранена в файл:\n{dialog.FileName}", "Успех");
                }
                catch (Exception ex)
                {
                    _uiService.ShowError($"Ошибка сохранения: {ex.Message}", "Ошибка");
                }
            }
        }
    }
}
