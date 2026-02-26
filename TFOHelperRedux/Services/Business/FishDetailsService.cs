using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media.Imaging;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Services.Business;

/// <summary>
/// Сервис для управления данными о выбранной рыбе (наживки, прикормки, рецепты, график клёва).
/// </summary>
public class FishDetailsService : INotifyPropertyChanged
{
    private readonly FishSelectionService _selectionService;
    private readonly MapsService _mapsService;
    private readonly FishFeedsViewModel _fishFeedsVM;

    private ObservableCollection<BaitModel>? _selectedFeeds;
    private ObservableCollection<BaitRecipeModel>? _selectedRecipes;
    private BitmapImage? _fishImage;

    public FishDetailsService(
        FishSelectionService selectionService,
        MapsService mapsService,
        FishFeedsViewModel fishFeedsVM)
    {
        _selectionService = selectionService;
        _mapsService = mapsService;
        _fishFeedsVM = fishFeedsVM;

        _selectedFeeds = new ObservableCollection<BaitModel>();
        _selectedRecipes = new ObservableCollection<BaitRecipeModel>();
        // RecipesForSelectedFish теперь возвращает _selectedRecipes
        MaybeCatchLures = new ObservableCollection<LureModel>();
        BestLures = new ObservableCollection<LureModel>();

        // Подписка на изменения выбора рыбы
        _selectionService.FishChanged += () =>
        {
            // Обновляем только изменившиеся данные
            UpdateSelectedFeedsEfficient();
            UpdateSelectedRecipesEfficient();
            UpdateRecipesForSelectedFish();
            UpdateMaybeCatchLures();
            UpdateBestLures();
            UpdateFishImage();
            // Одно уведомление для всех свойств
            OnPropertyChanged(nameof(MaybeCatchLures));
            OnPropertyChanged(nameof(BestLures));
            OnPropertyChanged(nameof(BiteDescription));
            OnPropertyChanged(nameof(RecipeCountForSelectedFish));
            OnPropertyChanged(nameof(SelectedFeeds));
            OnPropertyChanged(nameof(SelectedRecipes));
        };

        // Подписка на изменения точки лова (для обновления всех коллекций)
        DataStore.Selection.SelectionChanged += () =>
        {
            UpdateMaybeCatchLures();
            UpdateBestLures();
            UpdateSelectedFeedsEfficient();
            UpdateSelectedRecipesEfficient();
            UpdateRecipesForSelectedFish();
            
            OnPropertyChanged(nameof(MaybeCatchLures));
            OnPropertyChanged(nameof(BestLures));
            OnPropertyChanged(nameof(SelectedFeeds));
            OnPropertyChanged(nameof(SelectedRecipes));
            OnPropertyChanged(nameof(RecipesForSelectedFish));
        };

        // Подписка на изменения прикормок и рецептов из FishFeedsViewModel
        _fishFeedsVM.RecipeChanged += () =>
        {
            UpdateSelectedRecipesEfficient();
            UpdateRecipesForSelectedFish();
            OnPropertyChanged(nameof(SelectedRecipes));
            OnPropertyChanged(nameof(RecipesForSelectedFish));
            OnPropertyChanged(nameof(RecipeCountForSelectedFish));
        };

        _fishFeedsVM.FeedChanged += () =>
        {
            UpdateSelectedFeedsEfficient();
            OnPropertyChanged(nameof(SelectedFeeds));
        };
    }

    /// <summary>
    /// Выбранная рыба (делегирование в SelectionService)
    /// </summary>
    public FishModel? SelectedFish => _selectionService.SelectedFish;

    /// <summary>
    /// Изображение рыбы
    /// </summary>
    public BitmapImage? FishImage
    {
        get => _fishImage;
        private set
        {
            _fishImage = value;
            OnPropertyChanged(nameof(FishImage));
        }
    }

    /// <summary>
    /// Коллекция выбранных прикормок
    /// </summary>
    public ObservableCollection<BaitModel> SelectedFeeds => _selectedFeeds!;

    /// <summary>
    /// Коллекция выбранных рецептов
    /// </summary>
    public ObservableCollection<BaitRecipeModel> SelectedRecipes => _selectedRecipes!;

    /// <summary>
    /// Наживки, которые могут клевать (из данных рыбы)
    /// </summary>
    public ObservableCollection<LureModel> MaybeCatchLures { get; }

    /// <summary>
    /// Лучшие наживки (из точки лова)
    /// </summary>
    public ObservableCollection<LureModel> BestLures { get; }

    /// <summary>
    /// Эффективное обновление коллекции наживок "Может клевать"
    /// </summary>
    private void UpdateMaybeCatchLures()
    {
        if (MaybeCatchLures == null) return;

        var newLureIds = Enumerable.Empty<int>();

        // Используем LureIDs из выбранной точки лова
        var catchPoint = _selectionService.SelectedCatchPoint;
        if (catchPoint != null && catchPoint.LureIDs is { Length: > 0 })
        {
            newLureIds = catchPoint.LureIDs;
        }

        var newSet = new HashSet<int>(newLureIds);
        var existingSet = new HashSet<int>(MaybeCatchLures.Select(l => l.ID));

        // Удаляем наживки, которых больше нет
        for (int i = MaybeCatchLures.Count - 1; i >= 0; i--)
        {
            if (!newSet.Contains(MaybeCatchLures[i].ID))
                MaybeCatchLures.RemoveAt(i);
        }

        // Добавляем новые наживки
        foreach (var lureId in newLureIds)
        {
            if (!existingSet.Contains(lureId))
            {
                var lure = DataStore.Lures.FirstOrDefault(l => l.ID == lureId);
                if (lure != null)
                    MaybeCatchLures.Add(lure);
            }
        }
    }

    /// <summary>
    /// Эффективное обновление коллекции лучших наживок
    /// </summary>
    private void UpdateBestLures()
    {
        if (BestLures == null) return;

        var newLureIds = Enumerable.Empty<int>();

        // Используем BestLureIDs из выбранной точки лова
        var catchPoint = _selectionService.SelectedCatchPoint;
        if (catchPoint != null && catchPoint.BestLureIDs is { Length: > 0 })
        {
            newLureIds = catchPoint.BestLureIDs;
        }

        var newSet = new HashSet<int>(newLureIds);
        var existingSet = new HashSet<int>(BestLures.Select(l => l.ID));

        // Удаляем наживки, которых больше нет
        for (int i = BestLures.Count - 1; i >= 0; i--)
        {
            if (!newSet.Contains(BestLures[i].ID))
                BestLures.RemoveAt(i);
        }

        // Добавляем новые наживки
        foreach (var lureId in newLureIds)
        {
            if (!existingSet.Contains(lureId))
            {
                var lure = DataStore.Lures.FirstOrDefault(l => l.ID == lureId);
                if (lure != null)
                    BestLures.Add(lure);
            }
        }
    }

    /// <summary>
    /// Рецепты для выбранной рыбы
    /// </summary>
    public ObservableCollection<BaitRecipeModel> RecipesForSelectedFish => _selectedRecipes;

    /// <summary>
    /// Эффективное обновление коллекции рецептов для выбранной рыбы
    /// </summary>
    private void UpdateRecipesForSelectedFish()
    {
        // Используем тот же метод, что и для прикормок
        UpdateSelectedRecipesEfficient();
    }

    /// <summary>
    /// Количество рецептов для выбранной рыбы
    /// </summary>
    public int RecipeCountForSelectedFish => RecipesForSelectedFish.Count;

    /// <summary>
    /// Описание активности клёва
    /// </summary>
    public string BiteDescription
    {
        get
        {
            var fish = _selectionService.SelectedFish;
            if (fish?.BiteIntensity == null || fish.BiteIntensity.All(v => v == 0))
                return "Активность: нет данных";

            var activeRanges = new List<string>();
            int start = -1;

            for (int i = 0; i < fish.BiteIntensity.Length; i++)
            {
                bool isActive = fish.BiteIntensity[i] > 0;
                bool nextInactive = i == fish.BiteIntensity.Length - 1 || fish.BiteIntensity[i + 1] == 0;

                if (isActive && start == -1)
                    start = i;
                if (isActive && nextInactive && start != -1)
                {
                    activeRanges.Add(i == start ? $"{i}" : $"{start}–{i}");
                    start = -1;
                }
            }

            return "Активность: " + string.Join(", ", activeRanges) + " ч";
        }
    }

    /// <summary>
    /// Эффективное обновление коллекции прикормок
    /// </summary>
    private void UpdateSelectedFeedsEfficient()
    {
        if (_selectedFeeds == null) return;

        var newFeedIds = Enumerable.Empty<int>();

        // Используем FeedIDs из выбранной точки лова
        var catchPoint = _selectionService.SelectedCatchPoint;
        if (catchPoint != null && catchPoint.FeedIDs is { Length: > 0 })
        {
            newFeedIds = catchPoint.FeedIDs;
        }

        var newSet = new HashSet<int>(newFeedIds);
        var existingSet = new HashSet<int>(_selectedFeeds.Select(f => f.ID));

        // Удаляем прикормки, которых больше нет
        for (int i = _selectedFeeds.Count - 1; i >= 0; i--)
        {
            if (!newSet.Contains(_selectedFeeds[i].ID))
                _selectedFeeds.RemoveAt(i);
        }

        // Добавляем новые прикормки
        foreach (var feedId in newFeedIds)
        {
            if (!existingSet.Contains(feedId))
            {
                var feed = DataStore.Feeds.FirstOrDefault(f => f.ID == feedId);
                if (feed != null)
                    _selectedFeeds.Add(feed);
            }
        }
    }

    /// <summary>
    /// Эффективное обновление коллекции рецептов
    /// </summary>
    private void UpdateSelectedRecipesEfficient()
    {
        if (_selectedRecipes == null) return;

        var newRecipeIds = Enumerable.Empty<int>();

        // Используем RecipeIDs из выбранной точки лова
        var catchPoint = _selectionService.SelectedCatchPoint;
        if (catchPoint != null && catchPoint.RecipeIDs is { Length: > 0 })
        {
            newRecipeIds = catchPoint.RecipeIDs;
        }

        var newSet = new HashSet<int>(newRecipeIds);
        var existingSet = new HashSet<int>(_selectedRecipes.Select(r => r.ID));

        // Удаляем рецепты, которых больше нет
        for (int i = _selectedRecipes.Count - 1; i >= 0; i--)
        {
            if (!newSet.Contains(_selectedRecipes[i].ID))
                _selectedRecipes.RemoveAt(i);
        }

        // Добавляем новые рецепты
        foreach (var recipeId in newRecipeIds)
        {
            if (!existingSet.Contains(recipeId))
            {
                var recipe = DataStore.BaitRecipes.FirstOrDefault(r => r.ID == recipeId);
                if (recipe != null)
                    _selectedRecipes.Add(recipe);
            }
        }
    }

    /// <summary>
    /// Обновить изображение рыбы
    /// </summary>
    private void UpdateFishImage()
    {
        FishImage = _mapsService.GetFishImage(_selectionService.SelectedFish?.ID);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
