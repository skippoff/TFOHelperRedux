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

        // Подписка на изменения выбора рыбы
        _selectionService.FishChanged += () =>
        {
            UpdateSelectedFeeds();
            UpdateSelectedRecipes();
            UpdateFishImage();
            OnPropertyChanged(nameof(MaybeCatchLures));
            OnPropertyChanged(nameof(BestLures));
            OnPropertyChanged(nameof(BiteDescription));
            OnPropertyChanged(nameof(RecipeCountForSelectedFish));
            OnPropertyChanged(nameof(RecipesForSelectedFish));
        };

        // Подписка на изменения прикормок и рецептов из FishFeedsViewModel
        _fishFeedsVM.RecipeChanged += () =>
        {
            UpdateSelectedRecipes();
            OnPropertyChanged(nameof(SelectedRecipes));
            OnPropertyChanged(nameof(RecipesForSelectedFish));
            OnPropertyChanged(nameof(RecipeCountForSelectedFish));
        };

        _fishFeedsVM.FeedChanged += () =>
        {
            UpdateSelectedFeeds();
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
    /// Наживки, которые могут клевать
    /// </summary>
    public IEnumerable<LureModel> MaybeCatchLures
    {
        get
        {
            var fish = _selectionService.SelectedFish;
            if (fish?.LureIDs == null || fish.LureIDs.Length == 0)
                return Enumerable.Empty<LureModel>();

            if (DataStore.Lures == null || DataStore.Lures.Count == 0)
                return Enumerable.Empty<LureModel>();

            return DataStore.Lures.Where(l => fish.LureIDs.Contains(l.ID));
        }
    }

    /// <summary>
    /// Лучшие наживки
    /// </summary>
    public IEnumerable<LureModel> BestLures
    {
        get
        {
            var fish = _selectionService.SelectedFish;
            if (fish?.BestLureIDs == null || fish.BestLureIDs.Length == 0)
                return Enumerable.Empty<LureModel>();

            if (DataStore.Lures == null || DataStore.Lures.Count == 0)
                return Enumerable.Empty<LureModel>();

            return DataStore.Lures.Where(l => fish.BestLureIDs.Contains(l.ID));
        }
    }

    /// <summary>
    /// Рецепты для выбранной рыбы
    /// </summary>
    public IEnumerable<BaitRecipeModel> RecipesForSelectedFish
    {
        get
        {
            var fish = _selectionService.SelectedFish;
            if (fish?.RecipeIDs == null)
                return Enumerable.Empty<BaitRecipeModel>();

            return DataStore.BaitRecipes.Where(r => fish.RecipeIDs.Contains(r.ID));
        }
    }

    /// <summary>
    /// Количество рецептов для выбранной рыбы
    /// </summary>
    public int RecipeCountForSelectedFish
    {
        get
        {
            var fish = _selectionService.SelectedFish;
            if (fish?.RecipeIDs == null)
                return 0;

            return DataStore.BaitRecipes.Count(r => fish.RecipeIDs.Contains(r.ID));
        }
    }

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
    /// Обновить коллекцию выбранных прикормок
    /// </summary>
    private void UpdateSelectedFeeds()
    {
        if (_selectedFeeds == null) return;

        _selectedFeeds.Clear();
        var fish = _selectionService.SelectedFish;
        if (fish?.FeedIDs != null && fish.FeedIDs.Length > 0)
        {
            foreach (var feed in DataStore.Feeds.Where(f => fish.FeedIDs.Contains(f.ID)))
                _selectedFeeds.Add(feed);
        }
    }

    /// <summary>
    /// Обновить коллекцию выбранных рецептов
    /// </summary>
    private void UpdateSelectedRecipes()
    {
        if (_selectedRecipes == null) return;

        _selectedRecipes.Clear();
        var fish = _selectionService.SelectedFish;
        if (fish?.RecipeIDs != null && fish.RecipeIDs.Length > 0)
        {
            foreach (var recipe in DataStore.BaitRecipes.Where(r => fish.RecipeIDs.Contains(r.ID)))
                _selectedRecipes.Add(recipe);
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
