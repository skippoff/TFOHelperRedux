using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Serilog;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.State;

namespace TFOHelperRedux.Services.Data;

/// <summary>
/// Централизованное хранилище данных (статический класс для обратной совместимости)
/// </summary>
public static class DataStore
{
    private static readonly ILogger _log = Log.ForContext(typeof(DataStore));
    
    private static IDataLoadSaveService? _loadSaveService;
    private static SaveDebouncer? _saveDebouncer;
    private static SelectionState? _selection;
    private static ObservableCollection<MapModel>? _maps;
    private static ObservableCollection<FishModel>? _fishes;
    private static ObservableCollection<BaitModel>? _feeds;
    private static ObservableCollection<FeedComponentModel>? _feedComponents;
    private static ObservableCollection<BaitRecipeModel>? _baitRecipes;
    private static ObservableCollection<DipModel>? _dips;
    private static ObservableCollection<LureModel>? _lures;
    private static ObservableCollection<CatchPointModel>? _catchPoints;
    private static ObservableCollection<CatchPointModel>? _filteredPoints;
    private static string? _currentMode = "Maps";

    /// <summary>
    /// Инициализация хранилища через ServiceContainer
    /// </summary>
    public static void Initialize(IDataLoadSaveService loadSaveService, SaveDebouncer saveDebouncer)
    {
        _loadSaveService = loadSaveService;
        _saveDebouncer = saveDebouncer;
        _selection = new SelectionState();
        
        _maps = new ObservableCollection<MapModel>();
        _fishes = new ObservableCollection<FishModel>();
        _feeds = new ObservableCollection<BaitModel>();
        _feedComponents = new ObservableCollection<FeedComponentModel>();
        _baitRecipes = new ObservableCollection<BaitRecipeModel>();
        _dips = new ObservableCollection<DipModel>();
        _lures = new ObservableCollection<LureModel>();
        _catchPoints = new ObservableCollection<CatchPointModel>();
        _filteredPoints = new ObservableCollection<CatchPointModel>();
        
        _InitSelectionSaveHandlers();
    }

    /// <summary>
    /// Централизованное состояние выбора (рыба, карта, точка лова)
    /// </summary>
    public static SelectionState Selection => _selection ??= new SelectionState();

    public static ObservableCollection<MapModel> Maps => _maps ??= new ObservableCollection<MapModel>();
    public static ObservableCollection<FishModel> Fishes => _fishes ??= new ObservableCollection<FishModel>();
    public static ObservableCollection<BaitModel> Feeds => _feeds ??= new ObservableCollection<BaitModel>();
    public static ObservableCollection<FeedComponentModel> FeedComponents => _feedComponents ??= new ObservableCollection<FeedComponentModel>();
    public static ObservableCollection<BaitRecipeModel> BaitRecipes => _baitRecipes ??= new ObservableCollection<BaitRecipeModel>();
    public static ObservableCollection<DipModel> Dips => _dips ??= new ObservableCollection<DipModel>();
    public static ObservableCollection<LureModel> Lures => _lures ??= new ObservableCollection<LureModel>();
    public static ObservableCollection<CatchPointModel> CatchPoints => _catchPoints ??= new ObservableCollection<CatchPointModel>();
    public static ObservableCollection<CatchPointModel> FilteredPoints => _filteredPoints ??= new ObservableCollection<CatchPointModel>();
    
    public static Action<IItemModel>? AddToRecipe { get; set; }

    public static string CurrentMode
    {
        get => _currentMode ?? "Maps";
        set => _currentMode = value;
    }

    /// <summary>
    /// Загрузка всех данных
    /// </summary>
    public static void LoadAll()
    {
        _log.Information("Начало загрузки данных...");
        
        if (_loadSaveService == null)
        {
            _log.Debug("Создание DataLoadSaveService...");
            _loadSaveService = new DataLoadSaveService();
        }
        
        if (_saveDebouncer == null)
        {
            _log.Debug("Создание SaveDebouncer...");
            _saveDebouncer = new SaveDebouncer(_loadSaveService);
        }
        
        if (_selection == null)
        {
            _log.Debug("Создание SelectionState...");
            _selection = new SelectionState();
        }

        _log.Debug("Загрузка карт...");
        _maps = _loadSaveService.LoadMaps();
        
        _log.Debug("Загрузка рыб...");
        _fishes = _loadSaveService.LoadFishes();
        
        _log.Debug("Загрузка прикормок...");
        _feeds = _loadSaveService.LoadFeeds();
        
        _log.Debug("Загрузка компонентов прикормок...");
        _feedComponents = _loadSaveService.LoadFeedComponents();
        
        _log.Debug("Загрузка рецептов...");
        _baitRecipes = _loadSaveService.LoadBaitRecipes();
        
        _log.Debug("Загрузка дипов...");
        _dips = _loadSaveService.LoadDips();
        
        _log.Debug("Загрузка воблеров...");
        _lures = _loadSaveService.LoadLures();

        // Загрузка точек лова из локального файла
        var localDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Maps");
        var localCatchFile = Path.Combine(localDataDir, "CatchPoints_Local.json");

        if (!Directory.Exists(localDataDir))
        {
            _log.Debug("Создание папки точек лова: {Path}", localDataDir);
            Directory.CreateDirectory(localDataDir);
        }

        _log.Debug("Загрузка точек лова из {Path}", localCatchFile);
        var loaded = JsonService.Load<ObservableCollection<CatchPointModel>>(localCatchFile);
        _catchPoints = loaded ?? new ObservableCollection<CatchPointModel>();
        _filteredPoints = new ObservableCollection<CatchPointModel>(_catchPoints);

        AddToRecipe = null;
        _InitDerivedCollections();
        _InitSelectionSaveHandlers();
        
        _log.Information("Данные загруены: рыбы={Fishes}, карты={Maps}, прикормки={Feeds}, дипы={Dips}, воблеры={Lures}, рецепты={Recipes}, компоненты={Components}, точки лова={CatchPoints}",
            _fishes.Count, _maps.Count, _feeds.Count, _dips.Count, _lures.Count, _baitRecipes.Count, _feedComponents.Count, _catchPoints.Count);
    }

    /// <summary>
    /// Сохранение всех данных при выходе
    /// </summary>
    public static void SaveAll()
    {
        _log.Information("Начало сохранения данных...");
        
        var localDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Maps");
        var localCatchFile = Path.Combine(localDataDir, "CatchPoints_Local.json");
        
        _log.Debug("Сохранение точек лова в {Path}", localCatchFile);
        JsonService.Save(localCatchFile, CatchPoints);

        _log.Debug("Сохранение компонентов прикормок...");
        _loadSaveService?.SaveFeedComponents(FeedComponents);
        
        _log.Debug("Сохранение рецептов...");
        _loadSaveService?.SaveBaitRecipes(BaitRecipes);
        
        _log.Information("Данные сохранены");
    }

    private static void _InitDerivedCollections()
    {
        _filteredPoints = new ObservableCollection<CatchPointModel>(_catchPoints ?? new ObservableCollection<CatchPointModel>());
    }

    private static void _InitSelectionSaveHandlers()
    {
        if (_fishes == null || _feeds == null || _dips == null || _lures == null || _saveDebouncer == null)
            return;

        // Fishes
        _fishes.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
                foreach (var it in e.NewItems)
                    if (it is INotifyPropertyChanged npc)
                        npc.PropertyChanged += FishItem_PropertyChanged;

            if (e.OldItems != null)
                foreach (var it in e.OldItems)
                    if (it is INotifyPropertyChanged npc)
                        npc.PropertyChanged -= FishItem_PropertyChanged;
        };
        foreach (var f in _fishes)
            if (f is INotifyPropertyChanged npcF)
                npcF.PropertyChanged += FishItem_PropertyChanged;

        // Feeds
        _feeds.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
                foreach (var it in e.NewItems)
                    if (it is INotifyPropertyChanged npc)
                        npc.PropertyChanged += FeedItem_PropertyChanged;
            if (e.OldItems != null)
                foreach (var it in e.OldItems)
                    if (it is INotifyPropertyChanged npc)
                        npc.PropertyChanged -= FeedItem_PropertyChanged;
        };
        foreach (var b in _feeds)
            if (b is INotifyPropertyChanged npcB)
                npcB.PropertyChanged += FeedItem_PropertyChanged;

        // Dips
        _dips.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
                foreach (var it in e.NewItems)
                    if (it is INotifyPropertyChanged npc)
                        npc.PropertyChanged += DipItem_PropertyChanged;
            if (e.OldItems != null)
                foreach (var it in e.OldItems)
                    if (it is INotifyPropertyChanged npc)
                        npc.PropertyChanged -= DipItem_PropertyChanged;
        };
        foreach (var d in _dips)
            if (d is INotifyPropertyChanged npcD)
                npcD.PropertyChanged += DipItem_PropertyChanged;

        // Lures
        _lures.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
                foreach (var it in e.NewItems)
                    if (it is INotifyPropertyChanged npc)
                        npc.PropertyChanged += LureItem_PropertyChanged;
            if (e.OldItems != null)
                foreach (var it in e.OldItems)
                    if (it is INotifyPropertyChanged npc)
                        npc.PropertyChanged -= LureItem_PropertyChanged;
        };
        foreach (var l in _lures)
            if (l is INotifyPropertyChanged npcL)
                npcL.PropertyChanged += LureItem_PropertyChanged;
    }

    private static void FishItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FishModel.IsSelected))
            _saveDebouncer?.ScheduleSaveFishes();
    }

    private static void FeedItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BaitModel.IsSelected))
            _saveDebouncer?.ScheduleSaveFeeds();
    }

    private static void DipItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DipModel.IsSelected))
            _saveDebouncer?.ScheduleSaveDips();
    }

    private static void LureItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LureModel.IsSelected))
            _saveDebouncer?.ScheduleSaveLures();
    }
}
