using System.Collections.ObjectModel;
using System.IO;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Services.Data;

/// <summary>
/// Сервис загрузки и сохранения данных
/// </summary>
public class DataLoadSaveService : IDataLoadSaveService
{
    public string BaseDir => AppDomain.CurrentDomain.BaseDirectory;

    // Папки данных
    public string FishesDir => Path.Combine(BaseDir, "Fishes");
    public string MapsDir => Path.Combine(BaseDir, "Maps");
    public string FeedsDir => Path.Combine(BaseDir, "Feeds");
    public string DipsDir => Path.Combine(BaseDir, "Dips");
    public string LuresDir => Path.Combine(BaseDir, "Lures");
    public string TagsDir => Path.Combine(BaseDir, "Tags");
    public string FeedComponentsDir => Path.Combine(BaseDir, "FeedComponents");
    public string RecipesDir => Path.Combine(BaseDir, "Recipes");
    
    // Пути к JSON-файлам
    public string FishesJson => Path.Combine(FishesDir, "Fishes.json");
    public string MapsJson => Path.Combine(MapsDir, "Maps.json");
    public string FeedsJson => Path.Combine(FeedsDir, "Feeds.json");
    public string DipsJson => Path.Combine(DipsDir, "Dips.json");
    public string LuresJson => Path.Combine(LuresDir, "Lures.json");
    public string TagsJson => Path.Combine(TagsDir, "Tags.json");
    public string FeedComponentsJson => Path.Combine(FeedComponentsDir, "FeedComponents.json");
    public string BaitRecipesJson => Path.Combine(RecipesDir, "BaitRecipes.json");

    public void LoadAll()
    {
        // Загрузка данных делегирована конкретным методам
        // Вызывается при инициализации приложения
    }

    public void SaveAll()
    {
        // Глобальное сохранение всех данных
        // Используется при выходе из приложения
    }

    public ObservableCollection<FishModel> LoadFishes()
    {
        var list = JsonService.Load<ObservableCollection<FishModel>>(FishesJson)
                   ?? new ObservableCollection<FishModel>();

        // Автоматическая привязка картинок по ID + починка битых путей
        foreach (var fish in list)
        {
            if (fish.ID <= 0)
                continue;

            var hasValidImage =
                !string.IsNullOrWhiteSpace(fish.ImagePath) &&
                File.Exists(fish.ImagePath);

            if (hasValidImage)
                continue;

            var imgPath = GetFishImagePath(fish.ID);
            if (File.Exists(imgPath))
                fish.ImagePath = imgPath;
            else
                fish.ImagePath = string.Empty;
        }

        return list;
    }

    public ObservableCollection<MapModel> LoadMaps()
    {
        return JsonService.Load<ObservableCollection<MapModel>>(MapsJson)
               ?? new ObservableCollection<MapModel>();
    }

    public ObservableCollection<BaitModel> LoadFeeds()
    {
        return JsonService.Load<ObservableCollection<BaitModel>>(FeedsJson)
               ?? new ObservableCollection<BaitModel>();
    }

    public ObservableCollection<FeedComponentModel> LoadFeedComponents()
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

    public ObservableCollection<BaitRecipeModel> LoadBaitRecipes()
    {
        if (!Directory.Exists(RecipesDir))
            Directory.CreateDirectory(RecipesDir);

        return JsonService.Load<ObservableCollection<BaitRecipeModel>>(BaitRecipesJson)
               ?? new ObservableCollection<BaitRecipeModel>();
    }

    public ObservableCollection<DipModel> LoadDips()
    {
        return JsonService.Load<ObservableCollection<DipModel>>(DipsJson)
               ?? new ObservableCollection<DipModel>();
    }

    public ObservableCollection<LureModel> LoadLures()
    {
        return JsonService.Load<ObservableCollection<LureModel>>(LuresJson)
               ?? new ObservableCollection<LureModel>();
    }

    public ObservableCollection<TagModel> LoadTags()
    {
        return JsonService.Load<ObservableCollection<TagModel>>(TagsJson)
               ?? new ObservableCollection<TagModel>();
    }

    public void SaveFishes(ObservableCollection<FishModel> fishes)
    {
        JsonService.Save(FishesJson, fishes);
    }

    public void SaveFeeds(ObservableCollection<BaitModel> feeds)
    {
        JsonService.Save(FeedsJson, feeds);
    }

    public void SaveFeedComponents(ObservableCollection<FeedComponentModel> components)
    {
        JsonService.Save(FeedComponentsJson, components);
    }

    public void SaveBaitRecipes(ObservableCollection<BaitRecipeModel> recipes)
    {
        if (!Directory.Exists(RecipesDir))
            Directory.CreateDirectory(RecipesDir);

        JsonService.Save(BaitRecipesJson, recipes);
    }

    public void SaveDips(ObservableCollection<DipModel> dips)
    {
        JsonService.Save(DipsJson, dips);
    }

    public void SaveLures(ObservableCollection<LureModel> lures)
    {
        JsonService.Save(LuresJson, lures);
    }

    public void SaveMaps(ObservableCollection<MapModel> maps)
    {
        JsonService.Save(MapsJson, maps);
    }

    public void SaveCatchPoints(string path, ObservableCollection<CatchPointModel> catchPoints)
    {
        JsonService.Save(path, catchPoints);
    }

    // Пути к изображениям
    public string GetLureImagePath(int id) => Path.Combine(LuresDir, $"{id}.png");
    public string GetFeedImagePath(int id) => Path.Combine(FeedsDir, $"{id}.png");
    public string GetDipImagePath(int id) => Path.Combine(DipsDir, $"{id}.png");
    public string GetFishImagePath(int id) => Path.Combine(FishesDir, $"{id}.png");
    public string GetMapImagePath(int id) => Path.Combine(MapsDir, $"{id}.png");
}
