using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Business;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.DI;
using TFOHelperRedux.Services.UI;

namespace TFOHelperRedux.ViewModels
{
    /// <summary>
    /// ViewModel для управления рыбами, навигации и отображения данных.
    /// Бизнес-логика делегирована сервисам:
    /// - DataStore.Selection: выбор рыбы и синхронизация чекбоксов
    /// - FishFilterService: фильтрация и поиск
    /// - LureBindingService: привязка наживок к рыбам
    /// - FishDataService: CRUD операции с рыбами
    /// - NavigationService: навигация между режимами
    /// - MapsService: управление картами (фильтрация, обновление данных)
    /// - CatchPointsService: управление точками лова (фильтрация, CRUD, импорт/экспорт)
    /// </summary>
    public class FishViewModel : BaseViewModel
    {
        #region Сервисы

        private readonly FishFilterService _filterService;
        private readonly LureBindingService _lureBindingService;
        private readonly FishDataService _fishDataService;
        private readonly MapsService _mapsService;

        #endregion

        #region ViewModel для под-панелей

        public NavigationViewModel NavigationVM { get; }
        public BaitsViewModel BaitsVM { get; }
        public BaitRecipesViewModel BaitRecipesVM { get; }
        public CatchPointsViewModel CatchPointsVM { get; }

        #endregion

        #region Команды привязки наживок

        public ICommand AttachLureToFishCmd { get; }
        public ICommand DetachLureFromFishCmd { get; }
        public ICommand DeleteRecipeForeverCmd { get; }

        #endregion

        #region Коллекции данных

        public ObservableCollection<FishModel> Fishes => DataStore.Fishes;
        public ObservableCollection<FishModel> FilteredFishes => _filterService.FilteredFishes;

        // Карты для панели локаций (обычные + DLC) и фильтр по уровню
        public ObservableCollection<MapModel> MapsForFish => _mapsService.MapsForFish;
        public ObservableCollection<MapModel> Maps => DataStore.Maps;
        public ObservableCollection<MapModel> NonDlcMaps => _mapsService.NonDlcMaps;
        public ObservableCollection<MapModel> DlcMaps => _mapsService.DlcMaps;
        public ObservableCollection<int> MapLevels => _mapsService.MapLevels;

        #endregion

        #region Свойства навигации и режимов (делегированы в NavigationViewModel)

        public string CurrentMode
        {
            get => NavigationVM.CurrentMode;
            set => NavigationVM.CurrentMode = value;
        }

        public string BaitsSubMode
        {
            get => NavigationVM.BaitsSubMode;
            set => NavigationVM.BaitsSubMode = value;
        }

        public int SelectedLevelFilter
        {
            get => _mapsService.SelectedLevelFilter;
            set => _mapsService.SelectedLevelFilter = value;
        }

        #endregion

        #region Свойства выбора рыбы и карты

        /// <summary>
        /// Выбранная карта
        /// </summary>
        public MapModel? SelectedMap
        {
            get => DataStore.Selection.SelectedMap;
            set
            {
                DataStore.Selection.SetSelectedMap(value, DataStore.Fishes, _filterService.GetFilteredFishes(), DataStore.Lures);
                OnPropertyChanged(nameof(SelectedMap));
                OnPropertyChanged(nameof(SelectedFish));
                OnPropertyChanged(nameof(RecommendedLures));
                OnPropertyChanged(nameof(BiteDescription));
                OnPropertyChanged(nameof(RecipeCountForSelectedFish));
                OnPropertyChanged(nameof(RecipesForSelectedFish));
                _mapsService.UpdateMapsForFish(SelectedFish);
                CatchPointsVM.RefreshFilteredPoints(SelectedFish);
                UpdateFishDetails();
            }
        }

        /// <summary>
        /// Выбранная рыба
        /// </summary>
        public FishModel? SelectedFish
        {
            get => DataStore.Selection.SelectedFish;
            set
            {
                DataStore.Selection.SetSelectedFish(value, DataStore.Lures);
                OnPropertyChanged(nameof(SelectedFish));
                OnPropertyChanged(nameof(RecommendedLures));
                OnPropertyChanged(nameof(BiteDescription));
                OnPropertyChanged(nameof(RecipeCountForSelectedFish));
                OnPropertyChanged(nameof(RecipesForSelectedFish));
                _mapsService.UpdateMapsForFish(value);
                CatchPointsVM.RefreshFilteredPoints(value);
                UpdateFishDetails();
            }
        }

        #endregion

        #region Свойства для отображения данных

        public BitmapImage? FishImage { get; set; }

        public ObservableCollection<BaitModel> Feeds => DataStore.Feeds;
        public ObservableCollection<FeedComponentModel> Components => DataStore.FeedComponents;
        public ObservableCollection<DipModel> Dips => DataStore.Dips;
        public ObservableCollection<LureModel> Lures => DataStore.Lures;

        public int RecipeCountForSelectedFish =>
            SelectedFish == null || SelectedFish.RecipeIDs == null
                ? 0
                : DataStore.BaitRecipes.Count(r => SelectedFish.RecipeIDs.Contains(r.ID));

        public IEnumerable<BaitRecipeModel> RecipesForSelectedFish =>
            SelectedFish == null || SelectedFish.RecipeIDs == null
                ? Enumerable.Empty<BaitRecipeModel>()
                : DataStore.BaitRecipes.Where(r => SelectedFish.RecipeIDs.Contains(r.ID));

        public IEnumerable<LureModel> RecommendedLures
        {
            get
            {
                if (DataStore.Lures == null || DataStore.Lures.Count == 0)
                    return Enumerable.Empty<LureModel>();

                return DataStore.Lures.Where(l => l.IsSelected);
            }
        }

        public string BiteDescription
        {
            get
            {
                var fish = SelectedFish;
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

        #endregion

        #region Поиск и фильтрация

        public string SearchText
        {
            get => _filterService.SearchText;
            set
            {
                if (_filterService.SearchText != value)
                {
                    _filterService.SearchText = value;
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        #endregion

        #region Конструктор

        public FishViewModel(
            FishFilterService filterService,
            LureBindingService lureBindingService,
            FishDataService fishDataService,
            MapsService mapsService,
            NavigationViewModel navigationVM,
            BaitsViewModel baitsVM,
            BaitRecipesViewModel baitRecipesVM,
            CatchPointsViewModel catchPointsVM)
        {
            _filterService = filterService;
            _lureBindingService = lureBindingService;
            _fishDataService = fishDataService;
            _mapsService = mapsService;

            // Инициализация ViewModel для навигации и прикормок
            NavigationVM = navigationVM;
            BaitsVM = baitsVM;
            BaitRecipesVM = baitRecipesVM;
            CatchPointsVM = catchPointsVM;

            // Подписка на изменения режимов навигации
            NavigationVM.OnModeChanged += OnModeChanged;
            NavigationVM.OnBaitsSubModeChanged += OnBaitsSubModeChanged;

            // Подписка на изменения выбора в DataStore.Selection
            DataStore.Selection.SelectionChanged += () =>
            {
                OnPropertyChanged(nameof(SelectedFish));
                OnPropertyChanged(nameof(SelectedMap));
            };
            DataStore.Selection.LuresSynced += () => OnPropertyChanged(nameof(RecommendedLures));

            // Инициализация команд привязки
            AttachLureToFishCmd = new RelayCommand(AttachLureToFish);
            DetachLureFromFishCmd = new RelayCommand(DetachLureFromFish);
            DeleteRecipeForeverCmd = new RelayCommand(DeleteRecipeForever);

            // Подписка на изменения IsSelected у наживок
            SubscribeToLureChanges();

            // Инициализация фильтров карт
            _mapsService.InitializeMapFilters();

            // Выбор первой локации при старте и фильтрация рыб
            if (_mapsService.SelectedMap == null)
            {
                _mapsService.SelectFirstDlcMapIfNull();
                if (_mapsService.SelectedMap != null)
                {
                    SelectedMap = _mapsService.SelectedMap;
                }
            }
        }

        #endregion

        #region Обработчики изменений режимов

        private void OnModeChanged()
        {
            Requery();

            if (CurrentMode == NavigationViewModel.Modes.Fish)
            {
                DataStore.Selection.SelectedMap = null;
                CatchPointsVM.RefreshFilteredPoints(SelectedFish);
            }
            else if (CurrentMode == NavigationViewModel.Modes.Maps)
            {
                NavigateToMaps();
            }
        }

        private void OnBaitsSubModeChanged()
        {
            BaitsVM.SetCategory(NavigationVM.BaitsSubMode);
            Requery();
        }

        #endregion

        #region Методы навигации

        private void Requery() => CommandManager.InvalidateRequerySuggested();

        private void NavigateToMaps()
        {
            _mapsService.NavigateToMaps(
                () =>
                {
                    if (FilteredFishes.Cast<FishModel>().Any())
                        SelectedFish = FilteredFishes.Cast<FishModel>().First();
                },
                CatchPointsVM,
                SelectedFish
            );
        }

        #endregion

        #region Методы привязки наживок

        private void AttachLureToFish(object? parameter)
        {
            if (parameter is not LureModel lure)
                return;

            var result = _lureBindingService.AttachLureToFish(lure, SelectedFish);
            result.ShowMessageBox(ServiceContainer.GetService<IUIService>());
        }

        private void DetachLureFromFish(object? parameter)
        {
            if (parameter is not LureModel lure)
                return;

            var result = _lureBindingService.DetachLureFromFish(lure, SelectedFish);
            result.ShowMessageBox(ServiceContainer.GetService<IUIService>());
        }

        private void DeleteRecipeForever(object? parameter)
        {
            if (parameter is not BaitRecipeModel recipe)
                return;

            var uiService = ServiceContainer.GetService<IUIService>();
            var result = uiService.ShowMessageBox(
                $"Удалить рецепт \"{recipe.Name}\" только для текущей рыбы?",
                "Удаление рецепта для рыбы",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            var bindResult = _lureBindingService.RemoveRecipeFromFish(recipe, SelectedFish);
            bindResult.ShowMessageBox(uiService);

            if (bindResult.IsSuccess)
            {
                OnPropertyChanged(nameof(RecipesForSelectedFish));
                OnPropertyChanged(nameof(RecipeCountForSelectedFish));
                OnPropertyChanged(nameof(SelectedFish));
            }
        }

        #endregion

        #region Вспомогательные методы

        private void SubscribeToLureChanges()
        {
            if (DataStore.Lures == null)
                return;

            foreach (var lure in DataStore.Lures)
            {
                if (lure is INotifyPropertyChanged npc)
                    npc.PropertyChanged += LureModel_PropertyChanged;
            }

            DataStore.Lures.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (var it in e.NewItems)
                        if (it is INotifyPropertyChanged npc)
                            npc.PropertyChanged += LureModel_PropertyChanged;

                if (e.OldItems != null)
                    foreach (var it in e.OldItems)
                        if (it is INotifyPropertyChanged npc)
                            npc.PropertyChanged -= LureModel_PropertyChanged;
            };
        }

        private void LureModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(LureModel.IsSelected))
                return;

            if (DataStore.Selection.IsSyncingLures)
                return;

            if (sender is not LureModel lure)
                return;

            DataStore.Selection.HandleLureSelectionChanged(lure);
            OnPropertyChanged(nameof(RecommendedLures));
        }

        private void UpdateFishDetails()
        {
            FishImage = _mapsService.GetFishImage(SelectedFish?.ID);
            OnPropertyChanged(nameof(FishImage));
        }

        #endregion

        #region Публичные методы для обновления UI

        public void RefreshSelectedFish() => OnPropertyChanged(nameof(SelectedFish));
        public void RefreshRecommendedLures() => OnPropertyChanged(nameof(RecommendedLures));

        #endregion
    }
}
