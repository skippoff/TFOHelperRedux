using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Services;

public static class DataStore
{
    public static Action? OnMapWindowClosed;

    /// <summary>
    /// Централизованное состояние выбора (рыба, карта, точка лова)
    /// </summary>
    public static SelectionState Selection { get; } = new();

    public static void MapWindowClosed()
    {
        OnMapWindowClosed?.Invoke();
    }
    public static ObservableCollection<MapModel> Maps { get; private set; } = new();
    public static ObservableCollection<FishModel> Fishes { get; private set; } = new();
    public static ObservableCollection<BaitModel> Feeds { get; private set; } = new();
    public static ObservableCollection<FeedComponentModel> FeedComponents { get; private set; } = new();
    public static ObservableCollection<BaitRecipeModel> BaitRecipes { get; set; }
    public static ObservableCollection<DipModel> Dips { get; private set; } = new();
    public static ObservableCollection<LureModel> Lures { get; private set; } = new();
    public static ObservableCollection<TagModel> Tags { get; private set; } = new();
    public static ObservableCollection<CatchPointModel> CatchPoints { get; private set; } = new();
    // 🧭 Текущая выбранная точка лова (для редактирования)
    public static ObservableCollection<CatchPointModel> FilteredPoints { get; set; } = new();
    public static Action<IItemModel>? AddToRecipe { get; set; }
    private static string LocalDataDir => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Maps");
    private static string LocalCatchFile => Path.Combine(LocalDataDir, "CatchPoints_Local.json");


    public static void LoadAll()
    {
        if (!Directory.Exists(LocalDataDir))
            Directory.CreateDirectory(LocalDataDir);

        Maps = DataService.LoadMaps();
        Fishes = DataService.LoadFishes();
        Feeds = DataService.LoadFeeds();
        FeedComponents = DataService.LoadFeedComponents();
        BaitRecipes = DataService.LoadBaitRecipes();
        Dips = DataService.LoadDips();
        Lures = DataService.LoadLures();
        Tags = DataService.LoadTags();
        AddToRecipe = null;
        _InitDerivedCollections();

        // subscribe to selection changes to enable centralized debounced saving
        _InitSelectionSaveHandlers();

        var loaded = JsonService.Load<ObservableCollection<CatchPointModel>>(LocalCatchFile);
        CatchPoints = loaded ?? new ObservableCollection<CatchPointModel>();
    }

    private static void _InitSelectionSaveHandlers()
    {
        // Fishes
        Fishes.CollectionChanged += (s, e) =>
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
        foreach (var f in Fishes)
            if (f is INotifyPropertyChanged npcF)
                npcF.PropertyChanged += FishItem_PropertyChanged;

        // Feeds
        Feeds.CollectionChanged += (s, e) =>
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
        foreach (var b in Feeds)
            if (b is INotifyPropertyChanged npcB)
                npcB.PropertyChanged += FeedItem_PropertyChanged;

        // Dips
        Dips.CollectionChanged += (s, e) =>
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
        foreach (var d in Dips)
            if (d is INotifyPropertyChanged npcD)
                npcD.PropertyChanged += DipItem_PropertyChanged;

        // Lures
        Lures.CollectionChanged += (s, e) =>
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
        foreach (var l in Lures)
            if (l is INotifyPropertyChanged npcL)
                npcL.PropertyChanged += LureItem_PropertyChanged;
    }

    private static void FishItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Models.FishModel.IsSelected))
            SaveDebouncer.ScheduleSaveFishes();
    }

    private static void FeedItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Models.BaitModel.IsSelected))
            SaveDebouncer.ScheduleSaveFeeds();
    }

    private static void DipItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Models.DipModel.IsSelected))
            SaveDebouncer.ScheduleSaveDips();
    }

    private static void LureItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Models.LureModel.IsSelected))
            SaveDebouncer.ScheduleSaveLures();
    }
    // вызывать в конце LoadAll()
    private static void _InitDerivedCollections()
    {
        // по умолчанию фильтр = все точки
        if (CatchPoints != null)
            FilteredPoints = new ObservableCollection<CatchPointModel>(CatchPoints.ToList());
        else
            FilteredPoints = new ObservableCollection<CatchPointModel>();
    }

    public static void SaveAll()
    {
        JsonService.Save(LocalCatchFile, CatchPoints);
        DataService.SaveFeedComponents(FeedComponents);
        DataService.SaveBaitRecipes(BaitRecipes);
        if (App.Current.MainWindow?.DataContext is TFOHelperRedux.ViewModels.FishViewModel vm)
            vm.CatchPointsVM.RefreshFilteredPoints(Selection.SelectedFish);
    }

    public static string CurrentMode { get; set; } = "Maps";
}
