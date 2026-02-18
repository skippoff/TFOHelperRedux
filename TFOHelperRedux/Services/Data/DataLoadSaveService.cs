using System;
using System.Collections.ObjectModel;
using System.IO;
using Serilog;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Services.Data;

/// <summary>
/// Сервис загрузки и сохранения данных
/// </summary>
public class DataLoadSaveService : IDataLoadSaveService
{
    private static readonly ILogger _log = Serilog.Log.ForContext<DataLoadSaveService>();

    public string BaseDir => AppDomain.CurrentDomain.BaseDirectory;

    // Папки данных
    public string FishesDir => Path.Combine(BaseDir, "Fishes");
    public string MapsDir => Path.Combine(BaseDir, "Maps");
    public string FeedsDir => Path.Combine(BaseDir, "Feeds");
    public string DipsDir => Path.Combine(BaseDir, "Dips");
    public string LuresDir => Path.Combine(BaseDir, "Lures");
    public string FeedComponentsDir => Path.Combine(BaseDir, "FeedComponents");
    public string RecipesDir => Path.Combine(BaseDir, "Recipes");

    // Пути к JSON-файлам
    public string FishesJson => Path.Combine(FishesDir, "Fishes.json");
    public string MapsJson => Path.Combine(MapsDir, "Maps.json");
    public string FeedsJson => Path.Combine(FeedsDir, "Feeds.json");
    public string DipsJson => Path.Combine(DipsDir, "Dips.json");
    public string LuresJson => Path.Combine(LuresDir, "Lures.json");
    public string FeedComponentsJson => Path.Combine(FeedComponentsDir, "FeedComponents.json");
    public string BaitRecipesJson => Path.Combine(RecipesDir, "BaitRecipes.json");

    /// <summary>
    /// Generic-метод загрузки данных с опциональным пост-процессингом
    /// </summary>
    protected T LoadData<T>(string path, string entityName, Action<T>? postProcess = null) where T : System.Collections.ICollection, new()
    {
        _log.Debug("Загрузка {EntityName} из {Path}", entityName, path);

        var list = JsonService.Load<T>(path);
        
        if (list == null)
        {
            _log.Debug("{EntityName} не найдены, создан пустой список", entityName);
            list = new T();
        }

        _log.Debug("Загружено {Count} {EntityName}", list.Count, entityName);

        postProcess?.Invoke(list);

        return list;
    }

    /// <summary>
    /// Generic-метод сохранения данных
    /// </summary>
    protected void SaveData<T>(string path, T data, string entityName)
    {
        var count = (data as System.Collections.ICollection)?.Count ?? 0;
        _log.Debug("Сохранение {Count} {EntityName} в {Path}", count, entityName, path);
        JsonService.Save(path, data);
    }

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
        return LoadData<ObservableCollection<FishModel>>(FishesJson, "рыб", list =>
        {
            // Автоматическая привязка картинок по ID + починка битых путей
            foreach (var fish in list)
            {
                if (fish.ID <= 0)
                    continue;

                // Проверяем существование файла: сначала как абсолютный, потом как относительный
                var hasValidImage = !string.IsNullOrWhiteSpace(fish.ImagePath) &&
                    (File.Exists(fish.ImagePath) || File.Exists(Path.Combine(BaseDir, fish.ImagePath)));

                if (hasValidImage)
                {
                    // Нормализуем путь к абсолютному
                    if (!Path.IsPathRooted(fish.ImagePath))
                        fish.ImagePath = Path.Combine(BaseDir, fish.ImagePath);
                    continue;
                }

                var imgPath = GetFishImagePath(fish.ID);
                if (File.Exists(imgPath))
                {
                    fish.ImagePath = imgPath;
                    _log.Verbose("Восстановлен путь к изображению для рыбы {FishId}: {Path}", fish.ID, imgPath);
                }
                else
                    fish.ImagePath = string.Empty;
            }
        });
    }

    public ObservableCollection<MapModel> LoadMaps()
    {
        return LoadData<ObservableCollection<MapModel>>(MapsJson, "карт");
    }

    public ObservableCollection<BaitModel> LoadFeeds()
    {
        return LoadData<ObservableCollection<BaitModel>>(FeedsJson, "прикормок");
    }

    public ObservableCollection<FeedComponentModel> LoadFeedComponents()
    {
        return LoadData<ObservableCollection<FeedComponentModel>>(FeedComponentsJson, "компонентов", list =>
        {
            foreach (var comp in list)
            {
                if (comp.ID < 0)
                    continue;

                // Проверяем существование файла: сначала как абсолютный, потом как относительный
                var hasValidImage = !string.IsNullOrWhiteSpace(comp.ImagePath) &&
                    (File.Exists(comp.ImagePath) || File.Exists(Path.Combine(BaseDir, comp.ImagePath)));

                if (hasValidImage)
                {
                    // Нормализуем путь к абсолютному
                    if (!Path.IsPathRooted(comp.ImagePath))
                        comp.ImagePath = Path.Combine(BaseDir, comp.ImagePath);
                    continue;
                }

                var imgPath = Path.Combine(FeedComponentsDir, $"{comp.ID}.png");
                if (File.Exists(imgPath))
                {
                    comp.ImagePath = imgPath;
                    _log.Verbose("Восстановлен путь к изображению для компонента {ComponentId}: {Path}", comp.ID, imgPath);
                }
                else
                    comp.ImagePath = string.Empty;
            }
        });
    }

    public ObservableCollection<BaitRecipeModel> LoadBaitRecipes()
    {
        if (!Directory.Exists(RecipesDir))
        {
            _log.Debug("Создание папки рецептов: {Path}", RecipesDir);
            Directory.CreateDirectory(RecipesDir);
        }

        return LoadData<ObservableCollection<BaitRecipeModel>>(BaitRecipesJson, "рецептов");
    }

    public ObservableCollection<DipModel> LoadDips()
    {
        return LoadData<ObservableCollection<DipModel>>(DipsJson, "дипов");
    }

    public ObservableCollection<LureModel> LoadLures()
    {
        return LoadData<ObservableCollection<LureModel>>(LuresJson, "воблеров");
    }

    public void SaveFishes(ObservableCollection<FishModel> fishes)
    {
        SaveData(FishesJson, fishes, "рыб");
    }

    public void SaveFeeds(ObservableCollection<BaitModel> feeds)
    {
        SaveData(FeedsJson, feeds, "прикормок");
    }

    public void SaveFeedComponents(ObservableCollection<FeedComponentModel> components)
    {
        SaveData(FeedComponentsJson, components, "компонентов");
    }

    public void SaveBaitRecipes(ObservableCollection<BaitRecipeModel> recipes)
    {
        if (!Directory.Exists(RecipesDir))
        {
            _log.Debug("Создание папки рецептов: {Path}", RecipesDir);
            Directory.CreateDirectory(RecipesDir);
        }

        SaveData(BaitRecipesJson, recipes, "рецептов");
    }

    public void SaveDips(ObservableCollection<DipModel> dips)
    {
        SaveData(DipsJson, dips, "дипов");
    }

    public void SaveLures(ObservableCollection<LureModel> lures)
    {
        SaveData(LuresJson, lures, "воблеров");
    }

    public void SaveMaps(ObservableCollection<MapModel> maps)
    {
        SaveData(MapsJson, maps, "карт");
    }

    public void SaveCatchPoints(string path, ObservableCollection<CatchPointModel> catchPoints)
    {
        SaveData(path, catchPoints, "точек лова");
    }

    // Пути к изображениям
    public string GetLureImagePath(int id) => Path.Combine(LuresDir, $"{id}.png");
    public string GetFeedImagePath(int id) => Path.Combine(FeedsDir, $"{id}.png");
    public string GetDipImagePath(int id) => Path.Combine(DipsDir, $"{id}.png");
    public string GetFishImagePath(int id) => Path.Combine(FishesDir, $"{id}.png");
    public string GetMapImagePath(int id) => Path.Combine(MapsDir, $"{id}.png");
}
