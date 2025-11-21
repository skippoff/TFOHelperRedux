using System.Collections.ObjectModel;
using System.IO;
using TFOHelperRedux.Models;
using System.Text.Json;

namespace TFOHelperRedux.Services;

public static class DataService
{
    public static string BaseDir => AppDomain.CurrentDomain.BaseDirectory;

    // Папки данных
    public static string FishesDir => Path.Combine(BaseDir, "Fishes");
    public static string MapsDir => Path.Combine(BaseDir, "Maps");
    public static string FeedsDir => Path.Combine(BaseDir, "Feeds");
    public static string DipsDir => Path.Combine(BaseDir, "Dips");
    public static string LuresDir => Path.Combine(BaseDir, "Lures");
    public static string TagsDir => Path.Combine(BaseDir, "Tags");
    public static string FeedComponentsDir => Path.Combine(BaseDir, "FeedComponents");
    public static string RecipesDir => Path.Combine(BaseDir, "Recipes");
    public static string CraftLuresDir => Path.Combine(BaseDir, "CraftLures");

    // Пути к JSON-файлам (внутри папок)
    public static string CraftLuresJson => Path.Combine(CraftLuresDir, "CraftLures.json");
    public static string FishesJson => Path.Combine(FishesDir, "Fishes.json");
    public static string MapsJson => Path.Combine(MapsDir, "Maps.json");
    public static string FeedsJson => Path.Combine(FeedsDir, "Feeds.json");
    public static string DipsJson => Path.Combine(DipsDir, "Dips.json");
    public static string LuresJson => Path.Combine(LuresDir, "Lures.json");
    public static string TagsJson => Path.Combine(TagsDir, "Tags.json");
    public static string FeedComponentsJson => Path.Combine(FeedComponentsDir, "FeedComponents.json");
    public static string BaitRecipesJson => Path.Combine(RecipesDir, "BaitRecipes.json");
    // Загрузка рыб
    public static ObservableCollection<FishModel> LoadFishes()
    {
        var list = JsonService.Load<ObservableCollection<FishModel>>(FishesJson)
                   ?? new ObservableCollection<FishModel>();

        // Автоматическая привязка картинок по ID + починка битых путей
        foreach (var fish in list)
        {
            if (fish.ID <= 0)
                continue;

            // если в json путь есть, но файла по нему нет – считаем путь битым
            var hasValidImage =
                !string.IsNullOrWhiteSpace(fish.ImagePath) &&
                File.Exists(fish.ImagePath);

            if (hasValidImage)
                continue;

            // пробуем стандартный путь: <папка с exe>\Fishes\<ID>.png
            var imgPath = GetFishImagePath(fish.ID);
            if (File.Exists(imgPath))
                fish.ImagePath = imgPath;
            else
                fish.ImagePath = string.Empty;
        }

        return list;
    }

    public static ObservableCollection<CraftLureModel> LoadCraftLures()
    {
        try
        {
            // Если файла ещё нет – просто возвращаем пустой список
            if (!File.Exists(CraftLuresJson))
                return new ObservableCollection<CraftLureModel>();

            // Если файл есть, но он пустой – тоже не парсим
            var fileInfo = new FileInfo(CraftLuresJson);
            if (fileInfo.Length == 0)
                return new ObservableCollection<CraftLureModel>();

            // Пробуем загрузить через твой JsonService
            var list = JsonService.Load<ObservableCollection<CraftLureModel>>(CraftLuresJson);
            return list ?? new ObservableCollection<CraftLureModel>();
        }
        catch (JsonException)
        {
            // Если файл битый / невалидный – считаем, что крафтовых наживок пока нет
            // (при первом сохранении мы его перезапишем нормальными данными)
            return new ObservableCollection<CraftLureModel>();
        }
    }
    public static void SaveCraftLures(ObservableCollection<CraftLureModel> craftLures)
    {
        // гарантируем, что папка существует
        Directory.CreateDirectory(CraftLuresDir);

        JsonService.Save(CraftLuresJson, craftLures);
    }

    public static ObservableCollection<TagModel> LoadTags()
    {
        var list = JsonService.Load<ObservableCollection<TagModel>>(TagsJson)
                   ?? new ObservableCollection<TagModel>();
        return list;
    }
    // Загрузка карт
    public static ObservableCollection<MapModel> LoadMaps()
    {
        var list = JsonService.Load<ObservableCollection<MapModel>>(MapsJson)
                   ?? new ObservableCollection<MapModel>();
        return list;
    }

    // Загрузка прикормок
    public static ObservableCollection<BaitModel> LoadFeeds()
    {
        var list = JsonService.Load<ObservableCollection<BaitModel>>(FeedsJson)
                   ?? new ObservableCollection<BaitModel>();
        return list;
    }
    // Загрузка компонентов
    public static ObservableCollection<FeedComponentModel> LoadFeedComponents()
    {
        var list = JsonService.Load<ObservableCollection<FeedComponentModel>>(FeedComponentsJson)
                   ?? new ObservableCollection<FeedComponentModel>();

        foreach (var comp in list)
        {
            if (comp.ID < 0)
                continue;

            var hasValidImage =
                !string.IsNullOrWhiteSpace(comp.ImagePath) &&
                File.Exists(comp.ImagePath);

            if (hasValidImage)
                continue;

            var imgPath = Path.Combine(FeedComponentsDir, $"{comp.ID}.png");
            if (File.Exists(imgPath))
                comp.ImagePath = imgPath;
            else
                comp.ImagePath = string.Empty;
        }

        return list;
    }

    public static ObservableCollection<BaitRecipeModel> LoadBaitRecipes()
    {
        if (!Directory.Exists(RecipesDir))
            Directory.CreateDirectory(RecipesDir);

        return JsonService.Load<ObservableCollection<BaitRecipeModel>>(BaitRecipesJson)
               ?? new ObservableCollection<BaitRecipeModel>();
    }
    // Загрузка дипов
    public static ObservableCollection<DipModel> LoadDips()
    {
        var list = JsonService.Load<ObservableCollection<DipModel>>(DipsJson)
                   ?? new ObservableCollection<DipModel>();
        return list;
    }
    // Загрузка наживок
    public static ObservableCollection<LureModel> LoadLures()
    {
        var list = JsonService.Load<ObservableCollection<LureModel>>(LuresJson)
                   ?? new ObservableCollection<LureModel>();
        return list;
    }
    public static void SaveFishes(ObservableCollection<FishModel> fishes)
    {
        JsonService.Save(FishesJson, fishes);
    }
    public static void SaveFeeds(ObservableCollection<BaitModel> feeds)
    {
        JsonService.Save(FeedsJson, feeds);
    }
    public static void SaveFeedComponents(ObservableCollection<FeedComponentModel> components)
    {
        JsonService.Save(FeedComponentsJson, components);
    }
    public static void SaveBaitRecipes(ObservableCollection<BaitRecipeModel> recipes)
    {
        if (!Directory.Exists(RecipesDir))
            Directory.CreateDirectory(RecipesDir);

        JsonService.Save(BaitRecipesJson, recipes);
    }
    public static void SaveDips(ObservableCollection<DipModel> dips)
    {
        JsonService.Save(DipsJson, dips);
    }
    public static void SaveLures(ObservableCollection<LureModel> lures)
    {
        JsonService.Save(LuresJson, lures);
    }
    public static void SaveMaps(ObservableCollection<MapModel> maps)
    {
        JsonService.Save(MapsJson, maps);
    }
    // Путь к изображениям наживок
    public static string GetLureImagePath(int id) => Path.Combine(LuresDir, $"{id}.png");

    // Новые методы: прикормки и дипы
    public static string GetFeedImagePath(int id) => Path.Combine(FeedsDir, $"{id}.png");
    public static string GetDipImagePath(int id) => Path.Combine(DipsDir, $"{id}.png");

    // Пути к изображениям
    public static string GetFishImagePath(int id) => Path.Combine(FishesDir, $"{id}.png");
    public static string GetMapImagePath(int id) => Path.Combine(MapsDir, $"{id}.png");
}
