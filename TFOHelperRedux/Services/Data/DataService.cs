using System.Collections.ObjectModel;
using System.IO;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Services.Data;

/// <summary>
/// Сервис для получения путей к данным и изображениям
/// </summary>
public static class DataService
{
    public static string BaseDir => AppDomain.CurrentDomain.BaseDirectory;

    // Папки данных
    public static string FishesDir => Path.Combine(BaseDir, "Fishes");
    public static string MapsDir => Path.Combine(BaseDir, "Maps");
    public static string FeedsDir => Path.Combine(BaseDir, "Feeds");
    public static string DipsDir => Path.Combine(BaseDir, "Dips");
    public static string LuresDir => Path.Combine(BaseDir, "Lures");
    public static string FeedComponentsDir => Path.Combine(BaseDir, "FeedComponents");
    public static string RecipesDir => Path.Combine(BaseDir, "Recipes");

    // Пути к JSON-файлам
    public static string FishesJson => Path.Combine(FishesDir, "Fishes.json");
    public static string MapsJson => Path.Combine(MapsDir, "Maps.json");
    public static string FeedsJson => Path.Combine(FeedsDir, "Feeds.json");
    public static string DipsJson => Path.Combine(DipsDir, "Dips.json");
    public static string LuresJson => Path.Combine(LuresDir, "Lures.json");
    public static string FeedComponentsJson => Path.Combine(FeedComponentsDir, "FeedComponents.json");
    public static string BaitRecipesJson => Path.Combine(RecipesDir, "BaitRecipes.json");

    // Пути к изображениям
    public static string GetLureImagePath(int id) => Path.Combine(LuresDir, $"{id}.png");
    public static string GetFeedImagePath(int id) => Path.Combine(FeedsDir, $"{id}.png");
    public static string GetDipImagePath(int id) => Path.Combine(DipsDir, $"{id}.png");
    public static string GetFishImagePath(int id) => Path.Combine(FishesDir, $"{id}.png");
    public static string GetMapImagePath(int id) => Path.Combine(MapsDir, $"{id}.png");

    // Методы сохранения (для обратной совместимости)
    public static void SaveFishes(ObservableCollection<FishModel> fishes) =>
        JsonService.Save(FishesJson, fishes);

    public static void SaveFeeds(ObservableCollection<BaitModel> feeds) =>
        JsonService.Save(FeedsJson, feeds);

    public static void SaveFeedComponents(ObservableCollection<FeedComponentModel> components) =>
        JsonService.Save(FeedComponentsJson, components);

    public static void SaveBaitRecipes(ObservableCollection<BaitRecipeModel> recipes)
    {
        if (!Directory.Exists(RecipesDir))
            Directory.CreateDirectory(RecipesDir);
        JsonService.Save(BaitRecipesJson, recipes);
    }

    public static void SaveDips(ObservableCollection<DipModel> dips) =>
        JsonService.Save(DipsJson, dips);

    public static void SaveLures(ObservableCollection<LureModel> lures) =>
        JsonService.Save(LuresJson, lures);

    public static void SaveMaps(ObservableCollection<MapModel> maps) =>
        JsonService.Save(MapsJson, maps);
}
