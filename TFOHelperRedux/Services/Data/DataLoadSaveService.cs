using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
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
    /// Generic-метод загрузки данных в ObservableCollection с опциональным пост-процессингом.
    /// Десериализует в List{T} для корректной работы с System.Text.Json, затем создаёт ObservableCollection{T}.
    /// </summary>
    protected ObservableCollection<TItem> LoadData<TItem>(string path, string entityName, Action<ObservableCollection<TItem>>? postProcess = null)
    {
        _log.Debug("Загрузка {EntityName} из {Path}", entityName, path);

        ObservableCollection<TItem> collection;

        try
        {
            // Десериализуем в List<T> для надёжности, затем создаём ObservableCollection
            var list = JsonService.Load<List<TItem>>(path);

            if (list == null)
            {
                _log.Debug("{EntityName} не найдены, создана пустая коллекция", entityName);
                collection = new ObservableCollection<TItem>();
            }
            else
            {
                collection = new ObservableCollection<TItem>(list);
                _log.Debug("Загружено {Count} {EntityName}", collection.Count, entityName);
            }
        }
        catch (JsonException ex)
        {
            _log.Error(ex, "Ошибка десериализации {EntityName} из {Path}", entityName, path);
            collection = new ObservableCollection<TItem>();
        }
        catch (IOException ex)
        {
            _log.Error(ex, "Ошибка чтения файла {Path} для {EntityName}", path, entityName);
            collection = new ObservableCollection<TItem>();
        }

        postProcess?.Invoke(collection);

        return collection;
    }

    /// <summary>
    /// Generic-метод асинхронной загрузки данных в ObservableCollection с опциональным пост-процессингом.
    /// Десериализует в List{T} для корректной работы с System.Text.Json, затем создаёт ObservableCollection{T}.
    /// </summary>
    protected async Task<ObservableCollection<TItem>> LoadDataAsync<TItem>(string path, string entityName, Action<ObservableCollection<TItem>>? postProcess = null)
    {
        _log.Debug("Загрузка {EntityName} из {Path}", entityName, path);

        ObservableCollection<TItem> collection;

        try
        {
            // Десериализуем в List<T> для надёжности, затем создаём ObservableCollection
            var list = await JsonService.LoadAsync<List<TItem>>(path);

            if (list == null)
            {
                _log.Debug("{EntityName} не найдены, создана пустая коллекция", entityName);
                collection = new ObservableCollection<TItem>();
            }
            else
            {
                collection = new ObservableCollection<TItem>(list);
                _log.Debug("Загружено {Count} {EntityName}", collection.Count, entityName);
            }
        }
        catch (JsonException ex)
        {
            _log.Error(ex, "Ошибка десериализации {EntityName} из {Path}", entityName, path);
            collection = new ObservableCollection<TItem>();
        }
        catch (IOException ex)
        {
            _log.Error(ex, "Ошибка чтения файла {Path} для {EntityName}", path, entityName);
            collection = new ObservableCollection<TItem>();
        }

        postProcess?.Invoke(collection);

        return collection;
    }

    /// <summary>
    /// Generic-метод сохранения данных
    /// </summary>
    protected void SaveData<T>(string path, T data, string entityName)
    {
        var count = (data as System.Collections.ICollection)?.Count ?? 0;
        _log.Debug("Сохранение {Count} {EntityName} в {Path}", count, entityName, path);

        try
        {
            JsonService.Save(path, data);
        }
        catch (JsonException ex)
        {
            _log.Error(ex, "Ошибка сериализации {EntityName} в {Path}", entityName, path);
            throw;
        }
        catch (IOException ex)
        {
            _log.Error(ex, "Ошибка записи файла {Path} для {EntityName}", path, entityName);
            throw;
        }
    }

    /// <summary>
    /// Generic-метод асинхронного сохранения данных
    /// </summary>
    protected async Task SaveDataAsync<T>(string path, T data, string entityName)
    {
        var count = (data as System.Collections.ICollection)?.Count ?? 0;
        _log.Debug("Сохранение {Count} {EntityName} в {Path}", count, entityName, path);

        try
        {
            await JsonService.SaveAsync(path, data);
        }
        catch (JsonException ex)
        {
            _log.Error(ex, "Ошибка сериализации {EntityName} в {Path}", entityName, path);
            throw;
        }
        catch (IOException ex)
        {
            _log.Error(ex, "Ошибка записи файла {Path} для {EntityName}", path, entityName);
            throw;
        }
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

    /// <summary>
    /// Асинхронная загрузка всех данных
    /// </summary>
    public async Task LoadAllAsync()
    {
        _log.Information("Асинхронная загрузка всех данных...");
        
        // Загружаем все данные параллельно
        await Task.WhenAll(
            LoadFishesAsync(),
            LoadMapsAsync(),
            LoadFeedsAsync(),
            LoadFeedComponentsAsync(),
            LoadBaitRecipesAsync(),
            LoadDipsAsync(),
            LoadLuresAsync()
        );
        
        _log.Information("Все данные загружены");
    }

    /// <summary>
    /// Асинхронное сохранение всех данных
    /// </summary>
    public async Task SaveAllAsync()
    {
        _log.Information("Асинхронное сохранение всех данных...");
        
        // Сохраняем все данные параллельно
        var localDataDir = Path.Combine(BaseDir, "Maps");
        var localCatchFile = Path.Combine(localDataDir, "CatchPoints_Local.json");
        
        await Task.WhenAll(
            SaveFeedComponentsAsync(FeedComponentsJson, DataStore.FeedComponents, "компонентов"),
            SaveBaitRecipesAsync(BaitRecipesJson, DataStore.BaitRecipes, "рецептов"),
            SaveCatchPointsAsync(localCatchFile, DataStore.CatchPoints, "точек лова")
        );
        
        _log.Information("Все данные сохранены");
    }

    public ObservableCollection<FishModel> LoadFishes()
    {
        return LoadData<FishModel>(FishesJson, "рыб", list =>
        {
            // Автоматическая привязка картинок по ID + починка битых путей
            foreach (var fish in list)
            {
                if (fish.ID < 0)
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

    public async Task<ObservableCollection<FishModel>> LoadFishesAsync()
    {
        return await LoadDataAsync<FishModel>(FishesJson, "рыб", list =>
        {
            // Автоматическая привязка картинок по ID + починка битых путей
            foreach (var fish in list)
            {
                if (fish.ID < 0)
                    continue;

                var hasValidImage = !string.IsNullOrWhiteSpace(fish.ImagePath) &&
                    (File.Exists(fish.ImagePath) || File.Exists(Path.Combine(BaseDir, fish.ImagePath)));

                if (hasValidImage)
                {
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
        return LoadData<MapModel>(MapsJson, "карт");
    }

    public async Task<ObservableCollection<MapModel>> LoadMapsAsync()
    {
        return await LoadDataAsync<MapModel>(MapsJson, "карт");
    }

    public ObservableCollection<BaitModel> LoadFeeds()
    {
        return LoadData<BaitModel>(FeedsJson, "прикормок");
    }

    public async Task<ObservableCollection<BaitModel>> LoadFeedsAsync()
    {
        return await LoadDataAsync<BaitModel>(FeedsJson, "прикормок");
    }

    public ObservableCollection<FeedComponentModel> LoadFeedComponents()
    {
        return LoadData<FeedComponentModel>(FeedComponentsJson, "компонентов", list =>
        {
            foreach (var comp in list)
            {
                if (comp.ID < 0)
                    continue;

                var hasValidImage = !string.IsNullOrWhiteSpace(comp.ImagePath) &&
                    (File.Exists(comp.ImagePath) || File.Exists(Path.Combine(BaseDir, comp.ImagePath)));

                if (hasValidImage)
                {
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

    public async Task<ObservableCollection<FeedComponentModel>> LoadFeedComponentsAsync()
    {
        return await LoadDataAsync<FeedComponentModel>(FeedComponentsJson, "компонентов", list =>
        {
            foreach (var comp in list)
            {
                if (comp.ID < 0)
                    continue;

                var hasValidImage = !string.IsNullOrWhiteSpace(comp.ImagePath) &&
                    (File.Exists(comp.ImagePath) || File.Exists(Path.Combine(BaseDir, comp.ImagePath)));

                if (hasValidImage)
                {
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

        return LoadData<BaitRecipeModel>(BaitRecipesJson, "рецептов");
    }

    public async Task<ObservableCollection<BaitRecipeModel>> LoadBaitRecipesAsync()
    {
        if (!Directory.Exists(RecipesDir))
        {
            _log.Debug("Создание папки рецептов: {Path}", RecipesDir);
            Directory.CreateDirectory(RecipesDir);
        }

        return await LoadDataAsync<BaitRecipeModel>(BaitRecipesJson, "рецептов");
    }

    public ObservableCollection<DipModel> LoadDips()
    {
        return LoadData<DipModel>(DipsJson, "дипов");
    }

    public async Task<ObservableCollection<DipModel>> LoadDipsAsync()
    {
        return await LoadDataAsync<DipModel>(DipsJson, "дипов");
    }

    public ObservableCollection<LureModel> LoadLures()
    {
        return LoadData<LureModel>(LuresJson, "воблеров");
    }

    public async Task<ObservableCollection<LureModel>> LoadLuresAsync()
    {
        return await LoadDataAsync<LureModel>(LuresJson, "воблеров");
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

    // Async версии методов сохранения
    public async Task SaveFeedComponentsAsync(string path, ObservableCollection<FeedComponentModel> components, string entityName)
    {
        await SaveDataAsync(path, components, entityName);
    }

    public async Task SaveBaitRecipesAsync(string path, ObservableCollection<BaitRecipeModel> recipes, string entityName)
    {
        if (!Directory.Exists(RecipesDir))
        {
            _log.Debug("Создание папки рецептов: {Path}", RecipesDir);
            Directory.CreateDirectory(RecipesDir);
        }
        await SaveDataAsync(path, recipes, entityName);
    }

    public async Task SaveCatchPointsAsync(string path, ObservableCollection<CatchPointModel> catchPoints, string entityName)
    {
        await SaveDataAsync(path, catchPoints, entityName);
    }

    // Пути к изображениям
    public string GetLureImagePath(int id) => Path.Combine(LuresDir, $"{id}.png");
    public string GetFeedImagePath(int id) => Path.Combine(FeedsDir, $"{id}.png");
    public string GetDipImagePath(int id) => Path.Combine(DipsDir, $"{id}.png");
    public string GetFishImagePath(int id) => Path.Combine(FishesDir, $"{id}.png");
    public string GetMapImagePath(int id) => Path.Combine(MapsDir, $"{id}.png");
}
