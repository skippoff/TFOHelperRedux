using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;

namespace TFOHelperRedux.ViewModels;

/// <summary>
/// ViewModel для управления прикормками (обычными и крафтовыми) с привязкой к точке лова
/// </summary>
public class FishFeedsViewModel : BaseViewModel
{
    private string _searchText = string.Empty;
    private bool _showFeeds = true;
    private bool _showRecipes = true;
    private CatchPointModel? _catchPoint;

    public ICollectionView FeedsView { get; private set; } = null!;
    public ICollectionView RecipesView { get; private set; } = null!;

    public ObservableCollection<BaitModel> Feeds => DataStore.Feeds;
    public ObservableCollection<BaitRecipeModel> Recipes => DataStore.BaitRecipes;

    /// <summary>
    /// Событие для уведомления об изменении рецепта
    /// </summary>
    public event Action? RecipeChanged;

    /// <summary>
    /// Событие для уведомления об изменении прикормки
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
        // Подписка на изменения коллекции рецептов
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

            RecipesView.Refresh();
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

            FeedsView.Refresh();
            OnPropertyChanged(nameof(FeedsView));
        };
    }

    private void Recipe_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Игнорием IsSelected - теперь чекбоксы работают через конвертеры
        if (sender is not BaitRecipeModel recipe)
            return;

        if (e.PropertyName == nameof(BaitRecipeModel.Name))
        {
            RecipesView.Refresh();
            OnPropertyChanged(nameof(RecipesView));
            RecipeChanged?.Invoke();
        }
    }

    private void Feed_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Игнорируем IsSelected - теперь чекбоксы работают через конвертеры
        if (sender is not BaitModel feed)
            return;

        if (e.PropertyName == nameof(BaitModel.Name))
        {
            FeedsView.Refresh();
            FeedChanged?.Invoke();
        }
    }

    /// <summary>
    /// Устанавливает точку лова и синхронизирует чекбоксы
    /// </summary>
    public void SetCatchPoint(CatchPointModel? catchPoint)
    {
        _catchPoint = catchPoint;
        SyncCheckboxes();
    }

    /// <summary>
    /// Синхронизирует чекбоксы с данными точки лова
    /// </summary>
    private void SyncCheckboxes()
    {
        if (_catchPoint == null)
            return;

        // Принудительно уведомляем UI об изменении CatchPoint
        // Это заставит конвертеры пересчитать значения
        _catchPoint.OnPropertyChanged(nameof(_catchPoint.FeedIDs));
        _catchPoint.OnPropertyChanged(nameof(_catchPoint.RecipeIDs));
    }

    private void ApplyFilter()
    {
        FeedsView.Refresh();
        RecipesView.Refresh();
    }

    /// <summary>
    /// Переключение выбора прикормки/рецепта для точки лова
    /// </summary>
    public void ToggleFeedSelection(int id, bool isChecked, bool isRecipe = false)
    {
        if (_catchPoint == null)
            return;

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

        DataStore.SaveAll();
    }
}
