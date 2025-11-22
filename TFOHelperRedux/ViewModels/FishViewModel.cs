using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class FishViewModel : BaseViewModel
    {
        public ICommand ShowFeeds { get; }
        public ICommand ShowComponents { get; }
        public ICommand ShowDips { get; }
        public ICommand ShowLures { get; }
        public ICommand EditCurrentItemCommand { get; }
        public ICommand AddNewItemCommand { get; }
        public ICommand ShowBaits { get; }
        public ICommand ShowMaps { get; }
        public ICommand ShowFishes { get; }
        public ICommand ShowTopLiveLuresCmd { get; }
        public ICommand ShowTopArtificialLuresCmd { get; }
        public ICommand DeleteFishCommand { get; }
        public ICommand AttachLureToFishCmd { get; }
        public ICommand DetachLureFromFishCmd { get; }
        public ICommand DeleteRecipeForeverCmd { get; }
        private void Requery() => System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        private int _selectedCategoryId = 0; // 0 = все рыбы
        public int RecipeCountForSelectedFish =>
            SelectedFish == null || SelectedFish.RecipeIDs == null
                ? 0
                : DataStore.BaitRecipes.Count(r => SelectedFish.RecipeIDs.Contains(r.ID));
        public IEnumerable<BaitRecipeModel> RecipesForSelectedFish =>
            SelectedFish == null || SelectedFish.RecipeIDs == null
                ? Enumerable.Empty<BaitRecipeModel>()
                : DataStore.BaitRecipes.Where(r => SelectedFish.RecipeIDs.Contains(r.ID));


        private int GetNextId<T>(IEnumerable<T> collection) where T : IItemModel
        {
            if (!collection.Any())
                return 1;
            return collection.Max(x => x.ID) + 1;
        }
        public ObservableCollection<FishModel> Fishes { get; }
        public ObservableCollection<FishModel> FilteredFishes { get; private set; }
        public ObservableCollection<MapModel> MapsForFish { get; private set; }
        public ObservableCollection<MapModel> Maps { get; set; }
        // Карты для панели локаций (обычные + DLC) и фильтр по уровню
        public ObservableCollection<MapModel> NonDlcMaps { get; } = new();
        public ObservableCollection<MapModel> DlcMaps { get; } = new();
        public ObservableCollection<int> MapLevels { get; } = new();
        private int _selectedLevelFilter;
        public int SelectedLevelFilter
        {
            get => _selectedLevelFilter;
            set
            {
                if (_selectedLevelFilter != value)
                {
                    _selectedLevelFilter = value;
                    OnPropertyChanged(nameof(SelectedLevelFilter));
                    UpdateMapFilters();
                }
            }
        }
        public BaitRecipesViewModel BaitRecipesVM { get; } = new();
        public CatchPointsViewModel CatchPointsVM { get; } = new();
        // Добавь публичный враппер для обновления биндингов:
        public void RefreshSelectedFish() => OnPropertyChanged(nameof(SelectedFish));
        public void RefreshRecommendedLures() => OnPropertyChanged(nameof(RecommendedLures));
        public ObservableCollection<BaitModel> Feeds => DataStore.Feeds;
        public ObservableCollection<FeedComponentModel> Components => DataStore.FeedComponents;
        public ObservableCollection<DipModel> Dips => DataStore.Dips;
        public ObservableCollection<LureModel> Lures => DataStore.Lures;
        public ICollectionView LiveLuresView { get; }
        public ICollectionView ArtificialLuresView { get; }
        public ICollectionView CurrentTopLuresView =>
            TopLuresMode == "Lure" ? ArtificialLuresView : LiveLuresView;

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
                    OnPropertyChanged(nameof(TopLuresMode)); // тот же метод, что и у BaitsSubMode
                    OnPropertyChanged(nameof(CurrentTopLuresView));
                }
            }
        }

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

        private MapModel? _selectedMap;
        public MapModel? SelectedMap
        {
            get => _selectedMap;
            set
            {
                if (_selectedMap != value)
                {
                    _selectedMap = value;
                    OnPropertyChanged(nameof(SelectedMap));
                    // ✅ держим в синхроне глобальное состояние
                    DataStore.SelectedMap = _selectedMap;
                    FilterByMap();
                    if (FilteredFishes.Any())
                    {
                        SelectedFish = FilteredFishes.First();
                    }
                    else
                    {
                        SelectedFish = null;
                    }
                    // ✅ перезапускаем фильтр точек с учётом новой карты
                    CatchPointsVM.RefreshFilteredPoints(SelectedFish);
                }
            }
        }
        private FishModel? _selectedFish;
        public FishModel? SelectedFish
        {
            get => _selectedFish;
            set
            {
                if (_selectedFish != value)
                {
                    _selectedFish = value;
                    DataStore.SelectedFish = _selectedFish = value;
                    OnPropertyChanged(nameof(SelectedFish));
                    OnPropertyChanged(nameof(RecommendedLures)); // <<< ВАЖНО
                    OnPropertyChanged(nameof(BiteDescription)); // 🔹 обновляем текст
                    OnPropertyChanged(nameof(RecipeCountForSelectedFish));
                    OnPropertyChanged(nameof(RecipesForSelectedFish));
                    OnPropertyChanged(nameof(TopLuresForSelectedFish));
                    OnPropertyChanged(nameof(TopRecipesForSelectedFish));
                    // фильтруем точки под выбранную рыбу
                    CatchPointsVM.RefreshFilteredPoints(_selectedFish);
                    UpdateFishDetails();
                }
            }
        }
        public IEnumerable<LureModel> RecommendedLures
        {
            get
            {
                var ids = SelectedFish?.LureIDs;
                if (ids == null || ids.Length == 0)
                    return Enumerable.Empty<LureModel>();

                // фильтруем только отмеченные наживки
                return DataStore.Lures.Where(l => ids.Contains(l.ID));
            }
        }
        /// Лучшие магазинные наживки для выбранной рыбы (по BestLureIDs).
        public IEnumerable<LureModel> TopLuresForSelectedFish =>
            SelectedFish == null
                ? Enumerable.Empty<LureModel>()
                : DataStore.Lures.Where(l =>
                    SelectedFish.BestLureIDs != null &&
                    SelectedFish.BestLureIDs.Contains(l.ID));

        /// Лучшие крафтовые наживки (рецепты прикорма), привязанные как топовые.
        public IEnumerable<BaitRecipeModel> TopRecipesForSelectedFish =>
            SelectedFish == null
                ? Enumerable.Empty<BaitRecipeModel>()
                : DataStore.BaitRecipes.Where(r =>
                    SelectedFish.BestRecipeIDs != null &&
                    SelectedFish.BestRecipeIDs.Contains(r.ID));
        private void AttachLureToFish(object? parameter)
        {
            if (parameter is not LureModel lure)
                return;

            var fish = SelectedFish;
            if (fish == null)
            {
                MessageBox.Show(
                    "Сначала выберите рыбу в правой панели.",
                    "Привязка наживки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // гарантируем, что массив есть
            fish.BestLureIDs ??= Array.Empty<int>();

            // если уже есть в лучших — просто сообщаем
            if (fish.BestLureIDs.Contains(lure.ID))
            {
                MessageBox.Show(
                    $"Наживка «{lure.Name}» уже есть в списке лучших для рыбы «{fish.Name}».",
                    "Лучшие наживки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // добавляем ID наживки в лучшие
            fish.BestLureIDs = fish.BestLureIDs
                .Concat(new[] { lure.ID })
                .Distinct()
                .ToArray();

            // сохраняем изменения в Fishes.json
            DataService.SaveFishes(DataStore.Fishes);

            // обновляем биндинги
            OnPropertyChanged(nameof(TopLuresForSelectedFish));

            MessageBox.Show(
                $"Наживка «{lure.Name}» добавлена в лучшие для рыбы «{fish.Name}».",
                "Лучшие наживки",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void DetachLureFromFish(object? parameter)
        {
            if (parameter is not LureModel lure)
                return;

            var fish = SelectedFish;
            if (fish == null)
            {
                MessageBox.Show(
                    "Сначала выберите рыбу в правой панели.",
                    "Привязка наживки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (fish.BestLureIDs == null || fish.BestLureIDs.Length == 0)
            {
                MessageBox.Show(
                    $"У рыбы «{fish.Name}» ещё нет лучших наживок.",
                    "Лучшие наживки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (!fish.BestLureIDs.Contains(lure.ID))
            {
                MessageBox.Show(
                    $"Наживки «{lure.Name}» нет в списке лучших для рыбы «{fish.Name}».",
                    "Лучшие наживки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            fish.BestLureIDs = fish.BestLureIDs
                .Where(id => id != lure.ID)
                .ToArray();

            DataService.SaveFishes(DataStore.Fishes);

            OnPropertyChanged(nameof(TopLuresForSelectedFish));

            MessageBox.Show(
                $"Наживка «{lure.Name}» убрана из лучших для рыбы «{fish.Name}».",
                "Лучшие наживки",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        private void DeleteRecipeForever(object? parameter)
        {
            if (parameter is not BaitRecipeModel recipe)
                return;

            var result = MessageBox.Show(
                $"Полностью удалить рецепт \"{recipe.Name}\"?\n" +
                "Он будет удалён из всех рыб и из списка рецептов.",
                "Удаление рецепта",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            // 1) Убираем этот рецепт у всех рыб
            foreach (var fish in DataStore.Fishes)
            {
                if (fish.RecipeIDs == null || fish.RecipeIDs.Length == 0)
                    continue;

                fish.RecipeIDs = fish.RecipeIDs
                    .Where(id => id != recipe.ID)
                    .ToArray();
            }

            // 2) Убираем сам рецепт из общего списка
            if (DataStore.BaitRecipes != null && DataStore.BaitRecipes.Contains(recipe))
                DataStore.BaitRecipes.Remove(recipe);

            // 3) Сохраняем все данные (рыбы + рецепты)
            DataStore.SaveAll();

            // 4) Обновляем отображение рецептов для текущей рыбы
            OnPropertyChanged(nameof(SelectedFish));
            // если у тебя есть отдельное свойство типа SelectedFishRecipes, добавь и его:
            // OnPropertyChanged(nameof(SelectedFishRecipes));
        }
        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    ApplyFilter();
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }
        public BitmapImage? FishImage { get; private set; }
        // 🧩 Новая команда для кнопки "Добавить / Редактировать"
        public ICommand OpenAddEditFishWindowCommand { get; }
        public FishViewModel()
        {
            Fishes = DataStore.Fishes;
            FilteredFishes = new ObservableCollection<FishModel>(Fishes);
#if DEBUG
            EditCurrentItemCommand = new RelayCommand(EditCurrentItem, CanEditCurrentItem);
            AddNewItemCommand = new RelayCommand(AddNewItem, CanEditCurrentItem);
#else
            // В релизе команды есть, но скрыты и всегда неактивны
            EditCurrentItemCommand = new RelayCommand(_ => { }, _ => false);
            AddNewItemCommand      = new RelayCommand(_ => { }, _ => false);
#endif
            DeleteFishCommand = new RelayCommand(DeleteFish, CanDeleteFish);
            MapsForFish = new ObservableCollection<MapModel>();
            ShowMaps = new RelayCommand(() =>
            {
                CurrentMode = "Maps";

                // ✅ Если карта ещё не выбрана — выбираем первую
                if (SelectedMap == null && Maps.Any())
                {
                    SelectedMap = Maps.First();
                }

                // ✅ если рыбы под эту карту есть — выбираем первую
                if (FilteredFishes.Any())
                {
                    SelectedFish = FilteredFishes.First();
                }
            });
            ShowFishes = new RelayCommand(() =>
            {
                CurrentMode = "Fish";

                // ❌ убираем фильтр по карте: показываем все рыбы
                if (SelectedMap != null)
                    SelectedMap = null; // триггерит FilterByMap() и обновит список

                // держим в синхроне глобальное состояние
                TFOHelperRedux.Services.DataStore.SelectedMap = null;

                // если рыба не выбрана — выберем первую из полного списка
                if (SelectedFish == null && FilteredFishes.Any())
                    SelectedFish = FilteredFishes.First();

                // точки лова теперь — по выбранной рыбе без ограничений карты
                CatchPointsVM.RefreshFilteredPoints(SelectedFish);
            });

            ShowBaits = new RelayCommand(() => CurrentMode = "Baits");
            ShowFeeds = new RelayCommand(() => BaitsSubMode = "Feeds");
            ShowComponents = new RelayCommand(() => BaitsSubMode = "FeedComponents");
            ShowDips = new RelayCommand(() => BaitsSubMode = "Dips");
            ShowLures = new RelayCommand(() => BaitsSubMode = "Lures");
            ShowTopLiveLuresCmd = new RelayCommand(() => TopLuresMode = "Live");
            ShowTopArtificialLuresCmd = new RelayCommand(() => TopLuresMode = "Lure");
            AttachLureToFishCmd = new RelayCommand(AttachLureToFish);
            DetachLureFromFishCmd = new RelayCommand(DetachLureFromFish);
            DeleteRecipeForeverCmd = new RelayCommand(DeleteRecipeForever);

            Maps = DataStore.Maps; // Загружаем все карты из JSON
            TopLuresMode = "Live";// Создаём представление для фильтрации наживок
            
            // Плюс: все без Type тоже считаем живыми, чтобы ничего не потерялось
            LiveLuresView = CollectionViewSource.GetDefaultView(Lures);
            LiveLuresView.Filter = o =>
            {
                if (o is not LureModel l)
                    return false;

                if (string.IsNullOrWhiteSpace(l.BaitType))
                    return true; // без типа считаем живой наживкой

                return string.Equals(l.BaitType, "live", StringComparison.OrdinalIgnoreCase);
            };

            // Отдельное представление только искусственных приманок (lure)
            ArtificialLuresView = new ListCollectionView(Lures);
            ArtificialLuresView.Filter = o =>
            {
                if (o is not LureModel l)
                    return false;

                return string.Equals(l.BaitType, "lure", StringComparison.OrdinalIgnoreCase);
            };

            // подготавливаем коллекции для панели локаций
            MapLevels.Clear();
            if (Maps != null)
            {
                foreach (var lvl in Maps.Select(m => m.Level).Distinct().OrderBy(l => l))
                    MapLevels.Add(lvl);

                // по умолчанию — максимальный уровень, чтобы были видны все локации
                if (MapLevels.Any())
                    SelectedLevelFilter = MapLevels.Max();
            }
            // 🔹 При старте сразу выбираем первую локацию из списка,
            // если уже находимся в режиме "Maps"
            if (CurrentMode == "Maps" && SelectedMap == null)
            {
                // сначала обычные карты (не DLC)
                if (NonDlcMaps.Any())
                    SelectedMap = NonDlcMaps.First();
                // если обычных нет – берём первую из DLC
                else if (DlcMaps.Any())
                    SelectedMap = DlcMaps.First();
            }
            // Привязываем команду (только в DEBUG)
#if DEBUG
            OpenAddEditFishWindowCommand = new RelayCommand(OpenAddEditFishWindow);
#else
            // В релизе команда существует, но ничего не делает
            OpenAddEditFishWindowCommand = new RelayCommand(_ => { });
#endif
        }

        private void ApplyFilter()
        {
            IEnumerable<FishModel> filtered = Fishes;

            // 1️ фильтр по категории
            if (_selectedCategoryId > 0)
                filtered = filtered.Where(f => f.Tags != null && f.Tags.Contains(_selectedCategoryId));

            // 2️ фильтр по поиску
            if (!string.IsNullOrWhiteSpace(SearchText))
                filtered = filtered.Where(f => f.Name.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase));

            FilteredFishes.Clear();
            foreach (var f in filtered)
                FilteredFishes.Add(f);
        }
        public void FilterByCategory(int categoryId)
        {
            _selectedCategoryId = categoryId;
            ApplyFilter();
        }
        private void UpdateFishDetails()
        {
            MapsForFish.Clear();
            FishImage = null;

            if (SelectedFish == null)
            {
                OnPropertyChanged(nameof(FishImage));
                OnPropertyChanged(nameof(MapsForFish));
                return;
            }

            // 🖼️ Картинка рыбы
            var imgPath = DataService.GetFishImagePath(SelectedFish.ID);
            if (System.IO.File.Exists(imgPath))
                FishImage = new BitmapImage(new System.Uri(imgPath));

            // 🌍 Карты, где встречается
            var maps = DataStore.Maps
                .Where(m => m.FishIDs != null && m.FishIDs.Contains(SelectedFish.ID))
                .OrderBy(m => m.Name);

            foreach (var m in maps)
                MapsForFish.Add(m);

            OnPropertyChanged(nameof(FishImage));
            OnPropertyChanged(nameof(MapsForFish));
        }
        private bool CanDeleteFish(object? parameter)
        {
#if DEBUG
            return parameter is FishModel;
#else
            return false;
#endif
        }

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

            // 1. Удаляем рыбу из глобальной коллекции
            if (DataStore.Fishes.Contains(fish))
                DataStore.Fishes.Remove(fish);

            if (FilteredFishes.Contains(fish))
                FilteredFishes.Remove(fish);

            if (SelectedFish == fish)
            {
                SelectedFish = FilteredFishes.FirstOrDefault();
            }

            // 2. Удаляем ссылки на рыбу из карт
            foreach (var map in DataStore.Maps)
            {
                if (map.FishIDs != null && map.FishIDs.Contains(fish.ID))
                    map.FishIDs = map.FishIDs.Where(id => id != fish.ID).ToArray();
            }

            // 3. Удаляем ссылки на рыбу из рецептов прикормки
            foreach (var recipe in DataStore.BaitRecipes)
            {
                if (recipe.FishIDs != null && recipe.FishIDs.Contains(fish.ID))
                    recipe.FishIDs = recipe.FishIDs.Where(id => id != fish.ID).ToArray();
            }

            // 4. Удаляем ссылки на рыбу из точек лова
            foreach (var point in DataStore.CatchPoints)
            {
                if (point.FishIDs != null && point.FishIDs.Contains(fish.ID))
                    point.FishIDs = point.FishIDs.Where(id => id != fish.ID).ToArray();
            }

            // 5. Сохраняем изменения во все JSON'ы
            DataService.SaveFishes(DataStore.Fishes);   // Fishes.json
            DataService.SaveMaps(DataStore.Maps);       // карты
            DataStore.SaveAll();                        // прикормки, точки и т.п.

            OnPropertyChanged(nameof(Fishes));
            ApplyFilter();
#endif
        }
        // 🐟 Открывает окно добавления / редактирования рыбы
        private void OpenAddEditFishWindow()
        {
#if DEBUG
            // если рыба выбрана — передаём её напрямую по ссылке
            var fish = SelectedFish ?? new FishModel();
            var wnd = new AddFishToMapWindow(fish);

            if (wnd.ShowDialog() == true)
            {
                // если новая рыба — добавить
                if (!DataStore.Fishes.Contains(fish))
                    DataStore.Fishes.Add(fish);
                // обновить рекомендации после сохранения
                OnPropertyChanged(nameof(RecommendedLures));
                // ✅ сохраняем изменения прямо после закрытия окна
                DataService.SaveFishes(DataStore.Fishes);
            }
#endif
        }

        public string BiteDescription
        {
            get
            {
                var fish = SelectedFish;
                if (fish?.BiteIntensity == null || fish.BiteIntensity.All(v => v == 0))
                    return "Активность: нет данных";

                // ищем диапазоны часов, где интенсивность > 0
                var activeRanges = new List<string>();
                int start = -1;

                for (int i = 0; i < fish.BiteIntensity.Length; i++)
                {
                    bool isActive = fish.BiteIntensity[i] > 0;
                    bool nextInactive = i == fish.BiteIntensity.Length - 1 || fish.BiteIntensity[i + 1] == 0;

                    if (isActive && start == -1)
                        start = i; // начало диапазона
                    if (isActive && nextInactive && start != -1)
                    {
                        if (i == start)
                            activeRanges.Add($"{i}");
                        else
                            activeRanges.Add($"{start}–{i}");
                        start = -1;
                    }
                }

                return "Активность: " + string.Join(", ", activeRanges) + " ч";
            }
        }
        private string _currentMode = DataStore.CurrentMode; // стартуем так же, как в DataStore
        public string CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode != value)
                {
                    _currentMode = value;
                    TFOHelperRedux.Services.DataStore.CurrentMode = value;
                    OnPropertyChanged(nameof(CurrentMode));
                    Requery();

                    if (_currentMode == "Fish")
                    {
                        TFOHelperRedux.Services.DataStore.SelectedMap = null;
                        CatchPointsVM.RefreshFilteredPoints(SelectedFish);
                    }
                }
            }
        }
        private void UpdateMapFilters()
        {
            NonDlcMaps.Clear();
            DlcMaps.Clear();

            if (Maps == null || Maps.Count == 0)
                return;

            // Обычные карты
            var nonDlc = Maps.Where(m => !m.DLC);
            // DLC-карты
            var dlc = Maps.Where(m => m.DLC);

            // Фильтр по уровню только для обычных карт
            if (SelectedLevelFilter > 0)
                nonDlc = nonDlc.Where(m => m.Level <= SelectedLevelFilter);

            foreach (var map in nonDlc.OrderBy(m => m.Level).ThenBy(m => m.Name))
                NonDlcMaps.Add(map);

            foreach (var map in dlc.OrderBy(m => m.Level).ThenBy(m => m.Name))
                DlcMaps.Add(map);
        }
        private void FilterByMap()
        {
            // если SelectedMap не выбрана — показать всех
            if (SelectedMap == null)
            {
                FilteredFishes.Clear();
                foreach (var f in Fishes)
                    FilteredFishes.Add(f);
                return;
            }

            var fishOnMap = Fishes
                .Where(f => SelectedMap.FishIDs != null && SelectedMap.FishIDs.Contains(f.ID))
                .ToList();

            FilteredFishes.Clear();
            foreach (var fish in fishOnMap)
                FilteredFishes.Add(fish);
        }
#if DEBUG
        private bool CanEditCurrentItem()
        {
            if (CurrentMode != "Baits") return false;
            return BaitsSubMode is "Feeds" or "Dips" or "Lures" or "FeedComponents";
        }

        private void AddNewItem()
        {
            if (CurrentMode != "Baits") return;

            // Сбрасываем выбранный элемент в соответствующей вкладке оснастки,
            // чтобы EditCurrentItem создал новый
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
                    item = SelectedFeed ?? new BaitModel { ID = GetNextId(DataStore.Feeds), Name = "Новая прикормка" };
                    if (SelectedFeed == null) isNew = true;
                    break;
                case "Dips":
                    item = SelectedDip ?? new DipModel { ID = GetNextId(DataStore.Dips), Name = "Новый дип" };
                    if (SelectedDip == null) isNew = true;
                    break;
                case "Lures":
                    item = SelectedLure ?? new LureModel { ID = GetNextId(DataStore.Lures), Name = "Новая наживка" };
                    if (SelectedLure == null) isNew = true;
                    break;
                case "FeedComponents":
                    item = SelectedComponent ?? new FeedComponentModel { ID = GetNextId(DataStore.FeedComponents), Name = "Новый компонент" };
                    if (SelectedComponent == null) isNew = true;
                    break;
            }

            if (item == null) return;

            var wnd = new EditItemWindow(item)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            if (wnd.ShowDialog() == true)
            {
                // если новый элемент — добавляем в коллекцию
                if (isNew)
                {
                    switch (BaitsSubMode)
                    {
                        case "Feeds":
                            DataStore.Feeds.Add((BaitModel)item);
                            break;
                        case "Dips":
                            DataStore.Dips.Add((DipModel)item);
                            break;
                        case "Lures":
                            DataStore.Lures.Add((LureModel)item);
                            break;
                        case "FeedComponents":
                            DataStore.FeedComponents.Add((FeedComponentModel)item);
                            break;
                    }
                }

                // 💾 сохраняем соответствующую коллекцию
                if (item is BaitModel) DataService.SaveFeeds(DataStore.Feeds);
                else if (item is DipModel) DataService.SaveDips(DataStore.Dips);
                else if (item is LureModel) DataService.SaveLures(DataStore.Lures);
                else if (item is FeedComponentModel) DataService.SaveFeedComponents(DataStore.FeedComponents);

                OnPropertyChanged(nameof(Components)); // на случай внешних подписок
            }
        }
#endif

    }
}
