using System;
using System.Collections.ObjectModel;
using System.Linq;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Services;

/// <summary>
/// Сервис CRUD операций для рыб
/// </summary>
public class FishDataService
{
    private readonly IDataLoadSaveService _loadSaveService;

    public FishDataService(IDataLoadSaveService loadSaveService)
    {
        _loadSaveService = loadSaveService;
    }

    /// <summary>
    /// Получает следующий доступный ID для коллекции рыб
    /// </summary>
    public int GetNextFishId(ObservableCollection<FishModel> fishes)
    {
        if (!fishes.Any())
            return 1;
        return fishes.Max(x => x.ID) + 1;
    }

    /// <summary>
    /// Получает следующий доступный ID для коллекции
    /// </summary>
    public int GetNextId<T>(IEnumerable<T> collection) where T : IItemModel
    {
        if (!collection.Any())
            return 1;
        return collection.Max(x => x.ID) + 1;
    }

    /// <summary>
    /// Добавляет новую рыбу в коллекцию
    /// </summary>
    public FishModel AddNewFish(ObservableCollection<FishModel> fishes)
    {
        var newFish = new FishModel
        {
            ID = GetNextFishId(fishes),
            Name = "Новая рыба",
            BiteIntensity = Enumerable.Repeat(0, 24).ToArray()
        };

        fishes.Add(newFish);
        _loadSaveService.SaveFishes(fishes);

        return newFish;
    }

    /// <summary>
    /// Удаляет рыбу из всех коллекций
    /// </summary>
    public ServiceResult DeleteFish(FishModel fish)
    {
        if (!DataStore.Fishes.Contains(fish))
        {
            return ServiceResult.Failure("Рыба не найдена в коллекции.");
        }

        DataStore.Fishes.Remove(fish);

        foreach (var map in DataStore.Maps)
        {
            if (map.FishIDs != null && map.FishIDs.Contains(fish.ID))
            {
                map.FishIDs = map.FishIDs.Where(id => id != fish.ID).ToArray();
            }
        }

        foreach (var recipe in DataStore.BaitRecipes)
        {
            if (recipe.FishIDs != null && recipe.FishIDs.Contains(fish.ID))
            {
                recipe.FishIDs = recipe.FishIDs.Where(id => id != fish.ID).ToArray();
            }
        }

        foreach (var point in DataStore.CatchPoints)
        {
            if (point.FishIDs != null && point.FishIDs.Contains(fish.ID))
            {
                point.FishIDs = point.FishIDs.Where(id => id != fish.ID).ToArray();
            }
        }

        _loadSaveService.SaveFishes(DataStore.Fishes);
        _loadSaveService.SaveMaps(DataStore.Maps);
        DataStore.SaveAll();

        return ServiceResult.Success($"Рыба '{fish.Name}' удалена.");
    }

    /// <summary>
    /// Обновляет данные рыбы после редактирования
    /// </summary>
    public void UpdateFish(FishModel fish)
    {
        _loadSaveService.SaveFishes(DataStore.Fishes);
    }

    /// <summary>
    /// Получает рыбу по ID
    /// </summary>
    public FishModel? GetFishById(int id)
    {
        return DataStore.Fishes.FirstOrDefault(f => f.ID == id);
    }

    /// <summary>
    /// Создаёт или получает рыбу для редактирования
    /// </summary>
    public FishModel GetOrCreateFishForEdit(FishModel? selectedFish, ObservableCollection<FishModel> fishes)
    {
        return selectedFish ?? new FishModel
        {
            ID = GetNextFishId(fishes),
            Name = "Новая рыба",
            BiteIntensity = Enumerable.Repeat(0, 24).ToArray()
        };
    }

    /// <summary>
    /// Добавляет рыбу в коллекцию, если она ещё не добавлена
    /// </summary>
    public void AddFishIfNew(FishModel fish, ObservableCollection<FishModel> fishes)
    {
        if (!fishes.Contains(fish))
        {
            fishes.Add(fish);
        }
    }

    /// <summary>
    /// Сохраняет прикормку (BaitModel)
    /// </summary>
    public void SaveFeed(BaitModel feed)
    {
        _loadSaveService.SaveFeeds(DataStore.Feeds);
    }

    /// <summary>
    /// Сохраняет компонент прикормки
    /// </summary>
    public void SaveFeedComponent(FeedComponentModel component)
    {
        _loadSaveService.SaveFeedComponents(DataStore.FeedComponents);
    }

    /// <summary>
    /// Сохраняет дип
    /// </summary>
    public void SaveDip(DipModel dip)
    {
        _loadSaveService.SaveDips(DataStore.Dips);
    }

    /// <summary>
    /// Сохраняет воблер
    /// </summary>
    public void SaveLure(LureModel lure)
    {
        _loadSaveService.SaveLures(DataStore.Lures);
    }
}
