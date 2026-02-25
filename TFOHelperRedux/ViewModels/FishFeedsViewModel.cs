using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;

namespace TFOHelperRedux.ViewModels;

/// <summary>
/// ViewModel для управления прикормками (обычными и крафтовыми) с привязкой к рыбам
/// </summary>
public class FishFeedsViewModel : BaseViewModel
{
    private string _searchText = string.Empty;
    private bool _showFeeds = true;
    private bool _showRecipes = true;
    private CatchPointModel? _catchPoint;
    private bool _isCatchPointMode;

    public ICollectionView FeedsView { get; private set; } = null!;
    public ICollectionView RecipesView { get; private set; } = null!;

    public ObservableCollection<BaitModel> Feeds => DataStore.Feeds;
    public ObservableCollection<BaitRecipeModel> Recipes => DataStore.BaitRecipes;

    /// <summary>
    /// Событие для уведомления об изменении рецепта (для обновления FishViewModel)
    /// </summary>
    public event Action? RecipeChanged;
    
    /// <summary>
    /// Событие для уведомления об изменении прикормки (для обновления FishViewModel)
    /// </summary>
    public event Action? FeedChanged;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ApplyFilter();
            }
        }
    }

    public bool ShowFeeds
    {
        get => _showFeeds;
        set
        {
            if (_showFeeds != value)
            {
                _showFeeds = value;
                OnPropertyChanged(nameof(ShowFeeds));
                ApplyFilter();
            }
        }
    }

    public bool ShowRecipes
    {
        get => _showRecipes;
        set
        {
            if (_showRecipes != value)
            {
                _showRecipes = value;
                OnPropertyChanged(nameof(ShowRecipes));
                ApplyFilter();
            }
        }
    }

    public FishFeedsViewModel()
    {
        InitializeViews();
        SubscribeToCollectionChanges();
        // Начальная синхронизация состояния чекбоксов
        UpdateRecipesIsSelected();
        UpdateFeedsIsSelected();
    }

    private void InitializeViews()
    {
        // Представление для обычных прикормок
        FeedsView = CollectionViewSource.GetDefaultView(Feeds);
        FeedsView.Filter = f =>
        {
            if (f is not BaitModel feed)
                return false;
            if (string.IsNullOrWhiteSpace(SearchText))
                return ShowFeeds;
            return ShowFeeds && feed.Name.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase);
        };

        // Представление для рецептов
        RecipesView = CollectionViewSource.GetDefaultView(Recipes);
        RecipesView.Filter = r =>
        {
            if (r is not BaitRecipeModel recipe)
                return false;
            if (string.IsNullOrWhiteSpace(SearchText))
                return ShowRecipes;
            return ShowRecipes && recipe.Name.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase);
        };
    }

    private void SubscribeToCollectionChanges()
    {
        // Подписка на изменения коллекции рецептов для обновления IsSelected
        Recipes.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    if (item is BaitRecipeModel recipe)
                        recipe.PropertyChanged += Recipe_PropertyChanged;

            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    if (item is BaitRecipeModel recipe)
                        recipe.PropertyChanged -= Recipe_PropertyChanged;

            // Обновляем IsSelected для всех рецептов при изменении коллекции
            UpdateRecipesIsSelected();
            // Обновляем представление для отображения новых рецептов
            RecipesView.Refresh();
            // Уведомляем UI об изменении свойства
            OnPropertyChanged(nameof(RecipesView));
        };

        // Подписка на изменения коллекции прикормок
        Feeds.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    if (item is BaitModel feed)
                        feed.PropertyChanged += Feed_PropertyChanged;

            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    if (item is BaitModel feed)
                        feed.PropertyChanged -= Feed_PropertyChanged;

            // Обновляем IsSelected для всех прикормок при изменении коллекции
            UpdateFeedsIsSelected();
            // Обновляем представление для отображения новых прикормок
            FeedsView.Refresh();
            // Уведомляем UI об изменении свойства
            OnPropertyChanged(nameof(FeedsView));
        };

        // Подписка на изменения выбранной рыбы
        DataStore.Selection.SelectionChanged += Selection_SelectionChanged;
    }

    private bool _isUpdating = false;

    private void Recipe_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // При изменении IsSelected сохраняем изменения и обновляем UI
        if (sender is not BaitRecipeModel recipe || _isUpdating)
            return;

        if (e.PropertyName == nameof(BaitRecipeModel.IsSelected))
        {
            _isUpdating = true;
            try
            {
                // Передаём isRecipe=true для рецептов
                ToggleFeedSelection(recipe.ID, recipe.IsSelected, isRecipe: true);
                // Мгновенное обновление UI
                RecipesView.Refresh();
                OnPropertyChanged(nameof(RecipesView));
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }

    private void Feed_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // При изменении IsSelected сохраняем изменения и обновляем UI
        if (e.PropertyName == nameof(BaitModel.IsSelected) && sender is BaitModel feed && !_isUpdating)
        {
            _isUpdating = true;
            try
            {
                ToggleFeedSelection(feed.ID, feed.IsSelected, isRecipe: false);
                // Мгновенное обновление UI
                FeedsView.Refresh();
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }

    private void Selection_SelectionChanged()
    {
        // При смене выбранной рыбы обновляем состояние чекбоксов (только если не в режиме точки лова)
        if (!_isCatchPointMode)
        {
            UpdateRecipesIsSelected();
            UpdateFeedsIsSelected();
        }
    }

    /// <summary>
    /// Устанавливает режим работы с точкой лова
    /// </summary>
    public void SetCatchPoint(CatchPointModel? catchPoint)
    {
        _catchPoint = catchPoint;
        _isCatchPointMode = catchPoint != null;

        if (_isCatchPointMode)
        {
            // Проверяем, что RecipeIDs инициализирован
            if (_catchPoint.RecipeIDs == null)
                _catchPoint.RecipeIDs = Array.Empty<int>();
            
            // Обновляем состояние чекбоксов
            UpdateFeedsIsSelectedForCatchPoint();
            UpdateRecipesIsSelectedForCatchPoint();
        }
        else
        {
            UpdateRecipesIsSelected();
            UpdateFeedsIsSelected();
        }
    }
    
    /// <summary>
    /// Синхронизирует чекбоксы с данными рыбы
    /// </summary>
    public void SyncWithFish(FishModel fish)
    {
        if (fish == null) return;
        
        // Обновляем IsSelected для всех прикормок
        foreach (var feed in Feeds)
        {
            feed.IsSelected = fish.FeedIDs?.Contains(feed.ID) ?? false;
        }
        
        // Обновляем IsSelected для всех рецептов
        foreach (var recipe in Recipes)
        {
            recipe.IsSelected = fish.RecipeIDs?.Contains(recipe.ID) ?? false;
        }
    }

    private void UpdateRecipesIsSelected()
    {
        var fish = DataStore.Selection.SelectedFish;
        var recipeIdsSet = fish?.RecipeIDs != null && fish.RecipeIDs.Length > 0
            ? new HashSet<int>(fish.RecipeIDs)
            : null;

        foreach (var recipe in Recipes)
        {
            var shouldBeSelected = recipeIdsSet?.Contains(recipe.ID) ?? false;
            // Устанавливаем только если значение изменилось
            if (recipe.IsSelected != shouldBeSelected)
                recipe.IsSelected = shouldBeSelected;
        }
    }

    private void UpdateRecipesIsSelectedForCatchPoint()
    {
        var recipeIdsSet = _catchPoint?.RecipeIDs != null && _catchPoint.RecipeIDs.Length > 0
            ? new HashSet<int>(_catchPoint.RecipeIDs)
            : null;

        foreach (var recipe in Recipes)
        {
            var shouldBeSelected = recipeIdsSet?.Contains(recipe.ID) ?? false;
            if (recipe.IsSelected != shouldBeSelected)
                recipe.IsSelected = shouldBeSelected;
        }
    }

    private void UpdateFeedsIsSelected()
    {
        var fish = DataStore.Selection.SelectedFish;
        var feedIdsSet = fish?.FeedIDs != null && fish.FeedIDs.Length > 0
            ? new HashSet<int>(fish.FeedIDs)
            : null;

        foreach (var feed in Feeds)
        {
            var shouldBeSelected = feedIdsSet?.Contains(feed.ID) ?? false;
            // Устанавливаем только если значение изменилось
            if (feed.IsSelected != shouldBeSelected)
                feed.IsSelected = shouldBeSelected;
        }
    }

    private void UpdateFeedsIsSelectedForCatchPoint()
    {
        var feedIdsSet = _catchPoint?.FeedIDs != null && _catchPoint.FeedIDs.Length > 0
            ? new HashSet<int>(_catchPoint.FeedIDs)
            : null;

        foreach (var feed in Feeds)
        {
            var shouldBeSelected = feedIdsSet?.Contains(feed.ID) ?? false;
            if (feed.IsSelected != shouldBeSelected)
                feed.IsSelected = shouldBeSelected;
        }
    }

    private void ApplyFilter()
    {
        FeedsView.Refresh();
        RecipesView.Refresh();
    }

    /// <summary>
    /// Переключение выбора прикормки/рецепта для выбранной рыбы или точки лова
    /// </summary>
    public void ToggleFeedSelection(int id, bool isChecked, bool isRecipe = false)
    {
        if (_isCatchPointMode && _catchPoint != null)
        {
            // Режим точки лова
            if (isRecipe)
            {
                var recipeIds = _catchPoint.RecipeIDs?.ToList() ?? new List<int>();
                if (isChecked && !recipeIds.Contains(id))
                    recipeIds.Add(id);
                else if (!isChecked && recipeIds.Contains(id))
                    recipeIds.Remove(id);
                _catchPoint.RecipeIDs = recipeIds.Distinct().ToArray();
            }
            else
            {
                var feedIds = _catchPoint.FeedIDs?.ToList() ?? new List<int>();
                if (isChecked && !feedIds.Contains(id))
                    feedIds.Add(id);
                else if (!isChecked && feedIds.Contains(id))
                    feedIds.Remove(id);
                _catchPoint.FeedIDs = feedIds.Distinct().ToArray();
            }

            // Сохраняем точки лова через SaveDebouncer
            DataStore.SaveAll();
        }
        else
        {
            // Режим рыбы
            var fish = DataStore.Selection.SelectedFish;
            if (fish == null)
                return;

            if (isRecipe)
            {
                var recipeIds = fish.RecipeIDs?.ToList() ?? new List<int>();
                if (isChecked && !recipeIds.Contains(id))
                    recipeIds.Add(id);
                else if (!isChecked && recipeIds.Contains(id))
                    recipeIds.Remove(id);
                fish.RecipeIDs = recipeIds.Distinct().ToArray();
            }
            else
            {
                var feedIds = fish.FeedIDs?.ToList() ?? new List<int>();
                if (isChecked && !feedIds.Contains(id))
                    feedIds.Add(id);
                else if (!isChecked && feedIds.Contains(id))
                    feedIds.Remove(id);
                fish.FeedIDs = feedIds.Distinct().ToArray();
            }

            // Сохранение через SaveDebouncer (автоматически при изменении свойства)
            // Уведомляем об изменении
            if (isRecipe)
                RecipeChanged?.Invoke();
            else
                FeedChanged?.Invoke();
        }
    }
}
