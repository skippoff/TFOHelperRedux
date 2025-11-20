using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;

namespace TFOHelperRedux.ViewModels
{
    public class BaitRecipesViewModel : BaseViewModel
    {
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
            // гарантируем, что главная коллекция существует
            if (DataStore.BaitRecipes == null)
                DataStore.BaitRecipes = new ObservableCollection<BaitRecipeModel>();

            NormalizeRecipeIds();
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
                CurrentRecipe = new BaitRecipeModel { Name = "Новый рецепт" };

            switch (item)
            {
                case BaitModel feed:
                    if (!CurrentRecipe.FeedIDs.Contains(feed.ID))
                        CurrentRecipe.FeedIDs = CurrentRecipe.FeedIDs.Append(feed.ID).ToArray();
                    break;

                case LureModel lure:
                    if (!CurrentRecipe.LureIDs.Contains(lure.ID))
                        CurrentRecipe.LureIDs = CurrentRecipe.LureIDs.Append(lure.ID).ToArray();
                    break;

                case DipModel dip:
                    if (!CurrentRecipe.DipIDs.Contains(dip.ID))
                        CurrentRecipe.DipIDs = CurrentRecipe.DipIDs.Append(dip.ID).ToArray();
                    break;

                case FeedComponentModel comp:
                    if (!CurrentRecipe.ComponentIDs.Contains(comp.ID))
                        CurrentRecipe.ComponentIDs = CurrentRecipe.ComponentIDs.Append(comp.ID).ToArray();
                    break;
            }

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
            CurrentRecipe.DateEdited = DateTime.Now;

            var all = DataStore.BaitRecipes ??= new ObservableCollection<BaitRecipeModel>();

            // если рецепт ещё не в общей коллекции – это НОВЫЙ рецепт
            if (!all.Contains(CurrentRecipe))
            {
                int newId = all.Any() ? all.Max(r => r.ID) + 1 : 0;
                CurrentRecipe.ID = newId;
                all.Add(CurrentRecipe);
            }
            // если он уже в all – он там по ссылке, и мы уже изменили его поля выше

            DataService.SaveBaitRecipes(all);
            RebuildRecipesList();

            MessageBox.Show("Рецепт сохранён.", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NewRecipe()
        {
            CurrentRecipe = new BaitRecipeModel { Name = "Новый рецепт" };
            RecipeName = "";
            PreviewItems.Clear();
        }

        private void ClearRecipe()
        {
            if (CurrentRecipe == null) return;

            if (MessageBox.Show("Очистить текущий рецепт?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                CurrentRecipe.FeedIDs = Array.Empty<int>();
                CurrentRecipe.LureIDs = Array.Empty<int>();
                CurrentRecipe.DipIDs = Array.Empty<int>();
                CurrentRecipe.ComponentIDs = Array.Empty<int>();
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

            // ❌ Не удаляем из DataStore.BaitRecipes и не трогаем привязки к рыбе
            // ✅ Только помечаем как скрытый
            CurrentRecipe.IsHidden = true;

            DataService.SaveBaitRecipes(DataStore.BaitRecipes);
            RebuildRecipesList();
            NewRecipe();
        }

        private void AttachRecipeToFish(object? parameter)
        {
            if (parameter is not BaitRecipeModel recipe)
                return;

            var fish = DataStore.SelectedFish;
            if (fish == null)
            {
                MessageBox.Show("Сначала выберите рыбу в панели справа.", "Привязка рецепта",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // гарантируем, что массив есть
            fish.RecipeIDs ??= Array.Empty<int>();

            if (!fish.RecipeIDs.Contains(recipe.ID))
                fish.RecipeIDs = fish.RecipeIDs
                    .Concat(new[] { recipe.ID })
                    .Distinct()
                    .ToArray();

            DataService.SaveFishes(DataStore.Fishes);

            MessageBox.Show(
                $"Рецепт '{recipe.Name}' привязан к рыбе {fish.Name}.",
                "Привязка рецепта",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void DetachRecipeFromFish(object? parameter)
        {
            if (parameter is not BaitRecipeModel recipe)
                return;

            var fish = DataStore.SelectedFish;
            if (fish == null)
            {
                MessageBox.Show("Сначала выберите рыбу в панели справа.", "Отвязка рецепта",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (fish.RecipeIDs == null || fish.RecipeIDs.Length == 0)
                return;

            if (!fish.RecipeIDs.Contains(recipe.ID))
                return;

            fish.RecipeIDs = fish.RecipeIDs
                .Where(id => id != recipe.ID)
                .ToArray();

            DataService.SaveFishes(DataStore.Fishes);

            MessageBox.Show($"Рецепт '{recipe.Name}' отвязан от рыбы {fish.Name}.",
                "Отвязка рецепта",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void NormalizeRecipeIds()
        {
            var all = DataStore.BaitRecipes;
            if (all == null || all.Count == 0)
                return;

            // если ID не уникальны (или все 0) – переиндексируем
            var distinctCount = all.Select(r => r.ID).Distinct().Count();
            if (distinctCount != all.Count)
            {
                int id = 0;
                foreach (var r in all)
                {
                    r.ID = id++;
                }

                // сразу сохраняем исправленные ID в json
                DataService.SaveBaitRecipes(all);
            }
        }
    }
}
