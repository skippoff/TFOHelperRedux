using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Business;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.DI;
using TFOHelperRedux.Services.UI;
using TFOHelperRedux.Views;

namespace TFOHelperRedux.ViewModels
{
    /// <summary>
    /// ViewModel для управления режимом Fish.
    /// Координирует работу сервисов и предоставляет данные для View.
    /// Бизнес-логика делегирована сервисам:
    /// - FishSelectionService: выбор рыбы/карты
    /// - FishDetailsService: данные о рыбе (наживки, прикормки, рецепты)
    /// - FishLuresService: привязка наживок
    /// - FishNavigationService: навигация между режимами
    /// - MapsService: управление картами
    /// </summary>
    public class FishViewModel : BaseViewModel
    {
        #region Сервисы

        private readonly FishSelectionService _selectionService;
        private readonly FishDetailsService _detailsService;
        private readonly FishLuresService _luresService;
        private readonly FishNavigationService _navigationService;
        private readonly FishLuresCommandsService _commandsService;
        private readonly MapsService _mapsService;
        private readonly MapListViewService _mapListViewService;
        private readonly FishFilterService _filterService;

        #endregion

        #region ViewModel для под-панелей

        public NavigationViewModel NavigationVM { get; }
        public BaitsViewModel BaitsVM { get; }
        public BaitRecipesViewModel BaitRecipesVM { get; }
        public CatchPointsViewModel CatchPointsVM { get; }
        public FishFeedsViewModel FishFeedsVM { get; }

        #endregion

        #region Команды

        public ICommand AttachLureToFishCmd => _commandsService.AttachLureToFishCmd;
        public ICommand DetachLureFromFishCmd => _commandsService.DetachLureFromFishCmd;
        public ICommand DeleteRecipeForeverCmd => _commandsService.DeleteRecipeForeverCmd;
        public ICommand EditMapFishesCmd { get; }

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

        // Единый список карт с группировкой для ListBox
        public System.ComponentModel.ICollectionView AllMaps =>
            _mapListViewService.GetAllMapsView(Maps, SelectedLevelFilter);

        #endregion

        #region Свойства навигации и режимов

        public string CurrentMode => _navigationService.CurrentMode;

        public string BaitsSubMode => _navigationService.BaitsSubMode;

        public int SelectedLevelFilter
        {
            get => _mapsService.SelectedLevelFilter;
            set
            {
                if (_mapsService.SelectedLevelFilter != value)
                {
                    _mapsService.SelectedLevelFilter = value;
                    OnPropertyChanged(nameof(SelectedLevelFilter));
                    _mapListViewService.RefreshFilter(value);
                }
            }
        }

        #endregion

        #region Свойства выбора рыбы и карты

        public MapModel? SelectedMap
        {
            get => _selectionService.SelectedMap;
            set => _selectionService.SetSelectedMap(value);
        }

        public FishModel? SelectedFish
        {
            get => _selectionService.SelectedFish;
            set => _selectionService.SetSelectedFish(value);
        }

        public CatchPointModel? SelectedCatchPoint => _selectionService.SelectedCatchPoint;

        #endregion

        #region Свойства для отображения данных (делегирование в DetailsService)

        public System.Windows.Media.Imaging.BitmapImage? FishImage => _detailsService.FishImage;

        public ObservableCollection<BaitModel> Feeds => DataStore.Feeds;
        public ObservableCollection<FeedComponentModel> Components => DataStore.FeedComponents;
        public ObservableCollection<DipModel> Dips => DataStore.Dips;
        public ObservableCollection<LureModel> Lures => DataStore.Lures;

        public ObservableCollection<BaitModel> SelectedFeeds => _detailsService.SelectedFeeds;
        public ObservableCollection<BaitRecipeModel> SelectedRecipes => _detailsService.SelectedRecipes;

        public IEnumerable<LureModel> MaybeCatchLures => _detailsService.MaybeCatchLures;
        public IEnumerable<LureModel> BestLures => _detailsService.BestLures;

        public IEnumerable<BaitRecipeModel> RecipesForSelectedFish => _detailsService.RecipesForSelectedFish;
        public int RecipeCountForSelectedFish => _detailsService.RecipeCountForSelectedFish;

        public string BiteDescription => _detailsService.BiteDescription;

        public IEnumerable<BaitModel> SelectedCatchPointFeeds
        {
            get
            {
                var catchPoint = _selectionService.SelectedCatchPoint;
                if (catchPoint?.FeedIDs == null || catchPoint.FeedIDs.Length == 0)
                    return Enumerable.Empty<BaitModel>();

                if (DataStore.Feeds == null || DataStore.Feeds.Count == 0)
                    return Enumerable.Empty<BaitModel>();

                return DataStore.Feeds.Where(f => catchPoint.FeedIDs.Contains(f.ID));
            }
        }

        public IEnumerable<BaitRecipeModel> SelectedCatchPointRecipes
        {
            get
            {
                var catchPoint = _selectionService.SelectedCatchPoint;
                if (catchPoint?.RecipeIDs == null || catchPoint.RecipeIDs.Length == 0)
                    return Enumerable.Empty<BaitRecipeModel>();

                if (DataStore.BaitRecipes == null || DataStore.BaitRecipes.Count == 0)
                    return Enumerable.Empty<BaitRecipeModel>();

                return DataStore.BaitRecipes.Where(r => catchPoint.RecipeIDs.Contains(r.ID));
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
            MapListViewService mapListViewService,
            NavigationViewModel navigationVM,
            BaitsViewModel baitsVM,
            BaitRecipesViewModel baitRecipesVM,
            CatchPointsViewModel catchPointsVM,
            FishFeedsViewModel fishFeedsVM)
        {
            _filterService = filterService;
            _mapsService = mapsService;
            _mapListViewService = mapListViewService;

            // Инициализация ViewModel для навигации и прикормок
            NavigationVM = navigationVM;
            BaitsVM = baitsVM;
            BaitRecipesVM = baitRecipesVM;
            CatchPointsVM = catchPointsVM;
            FishFeedsVM = fishFeedsVM;

            // Создание сервисов
            _selectionService = new FishSelectionService();
            _detailsService = new FishDetailsService(_selectionService, mapsService, fishFeedsVM);
            _luresService = new FishLuresService(lureBindingService);
            _navigationService = new FishNavigationService(navigationVM, _selectionService, mapsService, catchPointsVM, filterService, baitsVM);
            _commandsService = new FishLuresCommandsService(_selectionService, lureBindingService);

            // Подписка на уведомления от сервисов
            SubscribeToServices();

            // Инициализация команд
            EditMapFishesCmd = new RelayCommand(EditMapFishes);

            // Инициализация фильтров карт
            _mapsService.InitializeMapFilters();

            // Выбор первой локации при старте
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

        #region Подписка на уведомления от сервисов

        private void SubscribeToServices()
        {
            // От FishSelectionService
            _selectionService.FishChanged += () =>
            {
                OnPropertyChanged(nameof(SelectedFish));
                OnPropertyChanged(nameof(SelectedMap));
                OnPropertyChanged(nameof(SelectedCatchPoint));
                OnPropertyChanged(nameof(SelectedCatchPointFeeds));
                OnPropertyChanged(nameof(SelectedCatchPointRecipes));
                OnPropertyChanged(nameof(FishImage));
            };

            _selectionService.MapChanged += () =>
            {
                OnPropertyChanged(nameof(SelectedMap));
                OnPropertyChanged(nameof(SelectedFish));
                _mapsService.UpdateMapsForFish(SelectedFish);
                CatchPointsVM.RefreshFilteredPoints(SelectedFish);
            };

            // От FishDetailsService
            _detailsService.PropertyChanged += (s, e) =>
            {
                OnPropertyChanged(e.PropertyName);
            };

            // От FishLuresService
            _luresService.LuresChanged += () =>
            {
                OnPropertyChanged(nameof(MaybeCatchLures));
                OnPropertyChanged(nameof(BestLures));
            };

            // От FishNavigationService
            _navigationService.ModeChanged += () =>
            {
                CommandManager.InvalidateRequerySuggested();

                if (CurrentMode == NavigationViewModel.Modes.Fish)
                {
                    CatchPointsVM.RefreshFilteredPoints(SelectedFish);
                }
            };
        }

        #endregion

        #region Вспомогательные методы

        private void EditMapFishes()
        {
#if DEBUG
            var map = SelectedMap;
            if (map == null)
            {
                UIService.ShowMessage("Сначала выбери локацию слева.", "Редактирование локации");
                return;
            }

            var win = new EditMapFishesWindow(map);
            UIService.ShowWindowModal(win, () =>
            {
                // После изменений обновляем список рыб по карте
                var current = SelectedMap;
                SelectedMap = null;
                SelectedMap = current;
            });
#endif
        }

        #endregion

        #region Публичные методы для обновления UI

        public void RefreshSelectedFish() => OnPropertyChanged(nameof(SelectedFish));
        public void RefreshMaybeCatchLures() => OnPropertyChanged(nameof(MaybeCatchLures));
        public void RefreshBestLures() => OnPropertyChanged(nameof(BestLures));

        #endregion
    }
}
