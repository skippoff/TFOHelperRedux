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
using TFOHelperRedux.Services;
using TFOHelperRedux.Views;

namespace TFOHelperRedux.ViewModels
{
    /// <summary>
    /// ViewModel для управления рыбами, навигации и отображения данных.
    /// Бизнес-логика делегирована сервисам:
    /// - FishSelectionService: выбор рыбы и синхронизация чекбоксов
    /// - FishFilterService: фильтрация и поиск
    /// - LureBindingService: привязка наживок к рыбам
    /// - FishDataService: CRUD операции с рыбами
    /// - LureService: CRUD операции с наживками, прикормками, дипами и компонентами
    /// - NavigationService: навигация между режимами
    /// - MapsService: управление картами (фильтрация, обновление данных)
    /// </summary>
    public class FishViewModel : BaseViewModel
    {
        #region Сервисы

        private readonly FishSelectionService _selectionService;
        private readonly FishFilterService _filterService;
        private readonly LureBindingService _lureBindingService;
        private readonly FishDataService _fishDataService;
        private readonly LureService _lureService;
        private readonly MapsService _mapsService;

        #endregion

        #region Команды навигации

        public ICommand ShowFeeds { get; }
        public ICommand ShowComponents { get; }
        public ICommand ShowDips { get; }
        public ICommand ShowLures { get; }
        public ICommand ShowBaits { get; }
        public ICommand ShowMaps { get; }
        public ICommand ShowFishes { get; }
        public ICommand ShowTopLiveLuresCmd { get; }
        public ICommand ShowTopArtificialLuresCmd { get; }

        #endregion

        #region Команды редактирования

        public ICommand EditCurrentItemCommand { get; }
        public ICommand AddNewItemCommand { get; }
        public ICommand DeleteFishCommand { get; }
        public ICommand OpenAddEditFishWindowCommand { get; }

        #endregion

        #region Команды привязки наживок

        public ICommand AttachLureToFishCmd { get; }
        public ICommand DetachLureFromFishCmd { get; }
        public ICommand DeleteRecipeForeverCmd { get; }

        #endregion

        #region Коллекции данных

        public ObservableCollection<FishModel> Fishes { get; }
        public ObservableCollection<FishModel> FilteredFishes { get; }
        public ObservableCollection<MapModel> MapsForFish => _mapsService.MapsForFish;
        public ObservableCollection<MapModel> Maps => _mapsService.Maps;

        // Карты для панели локаций (обычные + DLC) и фильтр по уровню
        public ObservableCollection<MapModel> NonDlcMaps => _mapsService.NonDlcMaps;
        public ObservableCollection<MapModel> DlcMaps => _mapsService.DlcMaps;
        public ObservableCollection<int> MapLevels => _mapsService.MapLevels;

        #endregion

        #region Свойства навигации и режимов

        public int SelectedLevelFilter
        {
            get => _mapsService.SelectedLevelFilter;
            set => _mapsService.SelectedLevelFilter = value;
        }

        private string _currentMode = DataStore.CurrentMode;
        public string CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode != value)
                {
                    _currentMode = value;
                    DataStore.CurrentMode = value;
                    OnPropertyChanged(nameof(CurrentMode));
                    Requery();

                    if (_currentMode == "Fish")
                    {
                        DataStore.SelectedMap = null;
                        CatchPointsVM.RefreshFilteredPoints(SelectedFish);
                    }
                }
            }
        }

        private string _baitsSubMode = "Feeds";
        public string BaitsSubMode
        {
            get => _baitsSubMode;
            set
            {
                if (_baitsSubMode != value)
                {
                    _baitsSubMode = value;
                    OnPropertyChanged(nameof(BaitsSubMode));
                    Requery();
                }
            }
        }

        private string _topLuresMode = "Live";
        public string TopLuresMode
        {
            get => _topLuresMode;
            set
            {
                if (_topLuresMode != value)
                {
                    _topLuresMode = value;
                    OnPropertyChanged(nameof(TopLuresMode));
                    OnPropertyChanged(nameof(CurrentTopLuresView));
                }
            }
        }

        #endregion

        #region Выбранные элементы (Baits)

        private FeedComponentModel? _selectedComponent;
        public FeedComponentModel? SelectedComponent
        {
            get => _selectedComponent;
            set { _selectedComponent = value; OnPropertyChanged(nameof(SelectedComponent)); Requery(); }
        }

        private BaitModel? _selectedFeed;
        public BaitModel? SelectedFeed
        {
            get => _selectedFeed;
            set { _selectedFeed = value; OnPropertyChanged(nameof(SelectedFeed)); }
        }

        private DipModel? _selectedDip;
        public DipModel? SelectedDip
        {
            get => _selectedDip;
            set { _selectedDip = value; OnPropertyChanged(nameof(SelectedDip)); }
        }

        private LureModel? _selectedLure;
        public LureModel? SelectedLure
        {
            get => _selectedLure;
            set { _selectedLure = value; OnPropertyChanged(nameof(SelectedLure)); }
        }

        #endregion

        #region Свойства выбора рыбы и карты (делегированы в сервис)

        /// <summary>
        /// Выбранная карта (делегировано в FishSelectionService)
        /// </summary>
        public MapModel? SelectedMap
        {
            get => _selectionService.SelectedMap;
            set => _selectionService.SetSelectedMap(value, Fishes, FilteredFishes);
        }

        /// <summary>
        /// Выбранная рыба (делегировано в FishSelectionService)
        /// </summary>
        public FishModel? SelectedFish
        {
            get => _selectionService.SelectedFish;
            set
            {
                _selectionService.SetSelectedFish(value, Lures);
                OnPropertyChanged(nameof(SelectedFish));
                OnPropertyChanged(nameof(RecommendedLures));
                OnPropertyChanged(nameof(BiteDescription));
                OnPropertyChanged(nameof(RecipeCountForSelectedFish));
                OnPropertyChanged(nameof(RecipesForSelectedFish));
                OnPropertyChanged(nameof(TopLuresForSelectedFish));
                OnPropertyChanged(nameof(TopRecipesForSelectedFish));
                _mapsService.UpdateMapsForFish(value);
                CatchPointsVM.RefreshFilteredPoints(value);
                UpdateFishDetails();
            }
        }

        #endregion

        #region Свойства для отображения данных

        public BitmapImage? FishImage { get; set; }

        public BaitRecipesViewModel BaitRecipesVM { get; } = new();
        public CatchPointsViewModel CatchPointsVM { get; } = new();

        public ObservableCollection<BaitModel> Feeds => DataStore.Feeds;
        public ObservableCollection<FeedComponentModel> Components => DataStore.FeedComponents;
        public ObservableCollection<DipModel> Dips => DataStore.Dips;
        public ObservableCollection<LureModel> Lures => DataStore.Lures;

        public ICollectionView LiveLuresView { get; }
        public ICollectionView ArtificialLuresView { get; }
        public ICollectionView CurrentTopLuresView =>
            TopLuresMode == "Lure" ? ArtificialLuresView : LiveLuresView;

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

        /// <summary>
        /// Лучшие магазинные наживки для выбранной рыбы (делегировано в LureBindingService)
        /// </summary>
        public IEnumerable<LureModel> TopLuresForSelectedFish =>
            _lureBindingService.GetTopLuresForFish(SelectedFish);

        /// <summary>
        /// Лучшие крафтовые рецепты для выбранной рыбы (делегировано в LureBindingService)
        /// </summary>
        public IEnumerable<BaitRecipeModel> TopRecipesForSelectedFish =>
            _lureBindingService.GetTopRecipesForFish(SelectedFish);

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

        public FishViewModel()
        {
            // Инициализация коллекций
            Fishes = DataStore.Fishes;
            FilteredFishes = new ObservableCollection<FishModel>(Fishes);

            // Инициализация сервисов
            _selectionService = new FishSelectionService(
                onSelectionChanged: () =>
                {
                    OnPropertyChanged(nameof(SelectedFish));
                    OnPropertyChanged(nameof(SelectedMap));
                },
                onLuresSynced: () => OnPropertyChanged(nameof(RecommendedLures))
            );

            _filterService = new FishFilterService(Fishes, FilteredFishes);
            _lureBindingService = new LureBindingService();
            _fishDataService = new FishDataService();
            _lureService = new LureService(_fishDataService);
            _mapsService = new MapsService(
                DataStore.Maps,
                onMapsChanged: () => OnPropertyChanged(nameof(Maps)),
                onSelectedMapChanged: () => OnPropertyChanged(nameof(SelectedMap)),
                onSelectedLevelFilterChanged: () => OnPropertyChanged(nameof(SelectedLevelFilter))
            );

            // Инициализация команд навигации
            ShowMaps = new RelayCommand(NavigateToMaps);
            ShowFishes = new RelayCommand(NavigateToFishes);
            ShowBaits = new RelayCommand(() => CurrentMode = "Baits");
            ShowFeeds = new RelayCommand(() => BaitsSubMode = "Feeds");
            ShowComponents = new RelayCommand(() => BaitsSubMode = "FeedComponents");
            ShowDips = new RelayCommand(() => BaitsSubMode = "Dips");
            ShowLures = new RelayCommand(() => BaitsSubMode = "Lures");
            ShowTopLiveLuresCmd = new RelayCommand(() => TopLuresMode = "Live");
            ShowTopArtificialLuresCmd = new RelayCommand(() => TopLuresMode = "Lure");

            // Инициализация команд привязки
            AttachLureToFishCmd = new RelayCommand(AttachLureToFish);
            DetachLureFromFishCmd = new RelayCommand(DetachLureFromFish);
            DeleteRecipeForeverCmd = new RelayCommand(DeleteRecipeForever);

            // Инициализация команд редактирования (только DEBUG)
#if DEBUG
            EditCurrentItemCommand = new RelayCommand(EditCurrentItem, CanEditCurrentItem);
            AddNewItemCommand = new RelayCommand(AddNewItem, CanEditCurrentItem);
            DeleteFishCommand = new RelayCommand(DeleteFish, CanDeleteFish);
            OpenAddEditFishWindowCommand = new RelayCommand(OpenAddEditFishWindow);
#else
            EditCurrentItemCommand = new RelayCommand(_ => { }, _ => false);
            AddNewItemCommand = new RelayCommand(_ => { }, _ => false);
            DeleteFishCommand = new RelayCommand(_ => { }, _ => false);
            OpenAddEditFishWindowCommand = new RelayCommand(_ => { });
#endif

            // Инициализация фильтров наживок
            TopLuresMode = "Live";

            LiveLuresView = CollectionViewSource.GetDefaultView(Lures);
            LiveLuresView.Filter = o =>
            {
                if (o is not LureModel l)
                    return false;

                if (string.IsNullOrWhiteSpace(l.BaitType))
                    return true;

                return string.Equals(l.BaitType, "live", StringComparison.OrdinalIgnoreCase);
            };

            ArtificialLuresView = new ListCollectionView(Lures);
            ArtificialLuresView.Filter = o =>
            {
                if (o is not LureModel l)
                    return false;

                return string.Equals(l.BaitType, "lure", StringComparison.OrdinalIgnoreCase);
            };

            // Подписка на изменения IsSelected у наживок
            SubscribeToLureChanges();

            // Инициализация фильтров карт
            _mapsService.InitializeMapFilters();

            // Выбор первой локации при старте в режиме Maps
            if (CurrentMode == "Maps" && _mapsService.SelectedMap == null)
            {
                _mapsService.SelectFirstDlcMapIfNull();
            }
        }

        #endregion

        #region Методы навигации

        private void Requery() => CommandManager.InvalidateRequerySuggested();

        private void NavigateToMaps()
        {
            CurrentMode = "Maps";
            _mapsService.NavigateToMaps(
                () =>
                {
                    if (FilteredFishes.Any())
                        SelectedFish = FilteredFishes.First();
                },
                CatchPointsVM,
                SelectedFish
            );
        }

        private void NavigateToFishes()
        {
            CurrentMode = "Fish";
            _mapsService.NavigateToFishes(
                () => SelectedMap = null,
                CatchPointsVM,
                SelectedFish,
                FilteredFishes
            );
        }

        #endregion

        #region Методы фильтрации

        public void FilterByCategory(int categoryId)
        {
            _filterService.FilterByCategory(categoryId);
        }

        private void ApplyFilter()
        {
            _filterService.ApplyFilter();
        }

        #endregion

        #region Методы привязки наживок

        private void AttachLureToFish(object? parameter)
        {
            if (parameter is not LureModel lure)
                return;

            var result = _lureBindingService.AttachLureToFish(lure, SelectedFish);
            result.ShowMessageBox();

            if (result.IsSuccess)
            {
                OnPropertyChanged(nameof(TopLuresForSelectedFish));
            }
        }

        private void DetachLureFromFish(object? parameter)
        {
            if (parameter is not LureModel lure)
                return;

            var result = _lureBindingService.DetachLureFromFish(lure, SelectedFish);
            result.ShowMessageBox();

            if (result.IsSuccess)
            {
                OnPropertyChanged(nameof(TopLuresForSelectedFish));
            }
        }

        private void DeleteRecipeForever(object? parameter)
        {
            if (parameter is not BaitRecipeModel recipe)
                return;

            var result = MessageBox.Show(
                $"Удалить рецепт \"{recipe.Name}\" только для текущей рыбы?",
                "Удаление рецепта для рыбы",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            var bindResult = _lureBindingService.RemoveRecipeFromFish(recipe, SelectedFish);
            bindResult.ShowMessageBox();

            if (bindResult.IsSuccess)
            {
                OnPropertyChanged(nameof(RecipesForSelectedFish));
                OnPropertyChanged(nameof(RecipeCountForSelectedFish));
                OnPropertyChanged(nameof(SelectedFish));
            }
        }

        #endregion

        #region Методы CRUD

        private void DeleteFish(object? parameter)
        {
#if DEBUG
            if (parameter is not FishModel fish)
                return;

            var result = MessageBox.Show(
                $"Удалить рыбу '{fish.Name}' из всех данных?",
                "Удаление рыбы",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            var deleteResult = _fishDataService.DeleteFish(fish);
            deleteResult.ShowMessageBox();

            if (FilteredFishes.Contains(fish))
                FilteredFishes.Remove(fish);

            if (SelectedFish == fish)
            {
                SelectedFish = FilteredFishes.FirstOrDefault();
            }

            OnPropertyChanged(nameof(Fishes));
            ApplyFilter();
#endif
        }

        private bool CanDeleteFish(object? parameter)
        {
#if DEBUG
            return parameter is FishModel;
#else
            return false;
#endif
        }

        private void OpenAddEditFishWindow()
        {
#if DEBUG
            var fish = _fishDataService.GetOrCreateFishForEdit(SelectedFish, Fishes);
            var isNew = !Fishes.Contains(fish);
            var wnd = new AddFishToMapWindow(fish);

            if (wnd.ShowDialog() == true)
            {
                if (isNew)
                    Fishes.Add(fish);

                _fishDataService.AddFishIfNew(fish, Fishes);
                OnPropertyChanged(nameof(RecommendedLures));
                DataService.SaveFishes(DataStore.Fishes);
            }
#endif
        }

        #endregion

        #region Методы редактирования элементов (Baits)

#if DEBUG
        private bool CanEditCurrentItem()
        {
            if (CurrentMode != "Baits") return false;
            return BaitsSubMode is "Feeds" or "Dips" or "Lures" or "FeedComponents";
        }

        private void AddNewItem()
        {
            if (CurrentMode != "Baits") return;

            switch (BaitsSubMode)
            {
                case "Feeds":
                    SelectedFeed = null;
                    break;
                case "Dips":
                    SelectedDip = null;
                    break;
                case "Lures":
                    SelectedLure = null;
                    break;
                case "FeedComponents":
                    SelectedComponent = null;
                    break;
            }

            EditCurrentItem();
        }

        private void EditCurrentItem()
        {
            if (CurrentMode != "Baits") return;

            IItemModel? item = null;
            bool isNew = false;

            switch (BaitsSubMode)
            {
                case "Feeds":
                    item = _lureService.GetOrCreateFeedForEdit(SelectedFeed);
                    if (SelectedFeed == null) isNew = true;
                    break;
                case "Dips":
                    item = _lureService.GetOrCreateDipForEdit(SelectedDip);
                    if (SelectedDip == null) isNew = true;
                    break;
                case "Lures":
                    item = _lureService.GetOrCreateLureForEdit(SelectedLure);
                    if (SelectedLure == null) isNew = true;
                    break;
                case "FeedComponents":
                    item = _lureService.GetOrCreateComponentForEdit(SelectedComponent);
                    if (SelectedComponent == null) isNew = true;
                    break;
            }

            if (item == null) return;

            var wnd = new EditItemWindow(item)
            {
                Owner = Application.Current.MainWindow
            };

            if (wnd.ShowDialog() == true)
            {
                if (isNew)
                {
                    switch (BaitsSubMode)
                    {
                        case "Feeds":
                            _lureService.AddFeedIfNew((BaitModel)item, DataStore.Feeds);
                            break;
                        case "Dips":
                            _lureService.AddDipIfNew((DipModel)item, DataStore.Dips);
                            break;
                        case "Lures":
                            _lureService.AddLureIfNew((LureModel)item, DataStore.Lures);
                            break;
                        case "FeedComponents":
                            _lureService.AddComponentIfNew((FeedComponentModel)item, DataStore.FeedComponents);
                            break;
                    }
                }

                _lureService.SaveItem(item);

                OnPropertyChanged(nameof(Components));
            }
        }
#endif

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

            if (_selectionService.IsSyncingLures)
                return;

            if (sender is not LureModel lure)
                return;

            _selectionService.HandleLureSelectionChanged(lure);
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
