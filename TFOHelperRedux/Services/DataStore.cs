using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Services;

public static class DataStore
{
    public static Action? OnMapWindowClosed;

    public static void MapWindowClosed()
    {
        OnMapWindowClosed?.Invoke();
    }
    public static ObservableCollection<MapModel> Maps { get; private set; } = new();
    public static ObservableCollection<FishModel> Fishes { get; private set; } = new();
    public static ObservableCollection<BaitModel> Feeds { get; private set; } = new();
    public static ObservableCollection<FeedComponentModel> FeedComponents { get; private set; } = new();
    public static ObservableCollection<BaitRecipeModel> BaitRecipes { get; set; }
    public static ObservableCollection<CraftLureModel> CraftLures { get; } = new();
    public static ObservableCollection<DipModel> Dips { get; private set; } = new();
    public static ObservableCollection<LureModel> Lures { get; private set; } = new();
    public static ObservableCollection<TagModel> Tags { get; private set; } = new();
    public static ObservableCollection<CatchPointModel> CatchPoints { get; private set; } = new();
    // 🧭 Текущая выбранная точка лова (для редактирования)
    public static ObservableCollection<CatchPointModel> FilteredPoints { get; set; } = new();
    public static CatchPointModel? SelectedCatchPoint { get; set; }
    public static Action<IItemModel>? AddToRecipe { get; set; }
    public static Action<IItemModel>? AddToCraftLure { get; set; }
    private static string LocalDataDir => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Maps");
    private static string LocalCatchFile => Path.Combine(LocalDataDir, "CatchPoints_Local.json");
    public static string BaitRecipesPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipes", "BaitRecipes.json");
    public static MapModel? SelectedMap { get; set; }
    public static FishModel? SelectedFish { get; set; }


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

        CraftLures.Clear();
        foreach (var cl in DataService.LoadCraftLures())
            CraftLures.Add(cl);

        var loaded = JsonService.Load<ObservableCollection<CatchPointModel>>(LocalCatchFile);
        CatchPoints = loaded ?? new ObservableCollection<CatchPointModel>();
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
        JsonService.Save(BaitRecipesPath, BaitRecipes);
        DataService.SaveBaitRecipes(BaitRecipes);
        DataService.SaveCraftLures(CraftLures);
        if (App.Current.MainWindow?.DataContext is TFOHelperRedux.ViewModels.FishViewModel vm)
            vm.CatchPointsVM.RefreshFilteredPoints(SelectedFish);
    }

    public static void ExportCatchPoints(string path) =>
        JsonService.Save(path, CatchPoints);

    public static void ImportCatchPoints(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Файл не найден.", "Импорт точек", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var imported = JsonService.Load<List<CatchPointModel>>(filePath);
            if (imported == null || imported.Count == 0)
            {
                MessageBox.Show("Не удалось загрузить данные из файла.", "Импорт точек", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CatchPoints.Count > 0)
            {
                // 🔸 спрашиваем, как поступить
                var result = MessageBox.Show(
                    "В программе уже есть точки лова.\n\n" +
                    "Объединить новые точки с существующими?\n" +
                    "Да — объединить\n" +
                    "Нет — заменить существующие\n" +
                    "Отмена — прервать импорт",
                    "Импорт точек",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                    return;

                if (result == MessageBoxResult.No)
                {
                    // Полностью заменяем
                    CatchPoints.Clear();
                    foreach (var p in imported)
                        CatchPoints.Add(p);
                }
                else if (result == MessageBoxResult.Yes)
                {
                    // Объединяем (добавляем только те, которых нет)
                    int added = 0;
                    foreach (var p in imported)
                    {
                        bool duplicate = CatchPoints.Any(existing =>
                            existing.MapID == p.MapID &&
                            existing.Coords.X == p.Coords.X &&
                            existing.Coords.Y == p.Coords.Y);

                        if (!duplicate)
                        {
                            CatchPoints.Add(p);
                            added++;
                        }
                    }

                    MessageBox.Show($"Импорт завершён. Добавлено {added} новых точек.",
                        "Импорт точек", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                // если точек не было — просто добавляем
                foreach (var p in imported)
                    CatchPoints.Add(p);
            }

            SaveAll();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка при импорте точек:\n" + ex.Message,
                            "Импорт точек", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    public static void ClearCatchPoints()
    {
        CatchPoints.Clear();
        SaveAll();
    }
    public static string CurrentMode { get; set; } = "Maps";
}
