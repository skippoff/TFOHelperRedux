using System;
using System.Collections.ObjectModel;
using System.Linq;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Services;

/// <summary>
/// Сервис CRUD операций для прикормок, дипов, воблеров и компонентов
/// </summary>
public class BaitCrudService
{
    private readonly FishDataService _dataService;

    public BaitCrudService(FishDataService dataService)
    {
        _dataService = dataService;
    }

    /// <summary>
    /// Создаёт новый элемент типа BaitModel
    /// </summary>
    public BaitModel CreateFeed()
    {
        return new BaitModel
        {
            ID = GetNextId(DataStore.Feeds),
            Name = "Новая прикормка"
        };
    }

    /// <summary>
    /// Создаёт новый элемент типа FeedComponentModel
    /// </summary>
    public FeedComponentModel CreateComponent()
    {
        return new FeedComponentModel
        {
            ID = GetNextId(DataStore.FeedComponents),
            Name = "Новый компонент"
        };
    }

    /// <summary>
    /// Создаёт новый элемент типа DipModel
    /// </summary>
    public DipModel CreateDip()
    {
        return new DipModel
        {
            ID = GetNextId(DataStore.Dips),
            Name = "Новый дип"
        };
    }

    /// <summary>
    /// Создаёт новый элемент типа LureModel
    /// </summary>
    public LureModel CreateLure()
    {
        return new LureModel
        {
            ID = GetNextId(DataStore.Lures),
            Name = "Новая наживка"
        };
    }

    /// <summary>
    /// Получает следующий доступный ID для коллекции
    /// </summary>
    public int GetNextId<T>(ObservableCollection<T> collection) where T : IItemModel
    {
        if (collection == null || collection.Count == 0)
            return 0;

        return collection.Max(item => item.ID) + 1;
    }

    /// <summary>
    /// Добавляет элемент в соответствующую коллекцию
    /// </summary>
    public void AddToCollection(IItemModel item)
    {
        switch (item)
        {
            case BaitModel feed:
                if (!DataStore.Feeds.Contains(feed))
                    DataStore.Feeds.Add(feed);
                break;
            case FeedComponentModel component:
                if (!DataStore.FeedComponents.Contains(component))
                    DataStore.FeedComponents.Add(component);
                break;
            case DipModel dip:
                if (!DataStore.Dips.Contains(dip))
                    DataStore.Dips.Add(dip);
                break;
            case LureModel lure:
                if (!DataStore.Lures.Contains(lure))
                    DataStore.Lures.Add(lure);
                break;
        }
    }

    /// <summary>
    /// Удаляет элемент из соответствующей коллекции
    /// </summary>
    public void RemoveFromCollection(IItemModel item)
    {
        switch (item)
        {
            case BaitModel feed:
                DataStore.Feeds.Remove(feed);
                break;
            case FeedComponentModel component:
                DataStore.FeedComponents.Remove(component);
                break;
            case DipModel dip:
                DataStore.Dips.Remove(dip);
                break;
            case LureModel lure:
                DataStore.Lures.Remove(lure);
                break;
        }
    }

    /// <summary>
    /// Сохраняет элемент в JSON-файл
    /// </summary>
    public void SaveItem(IItemModel item)
    {
        switch (item)
        {
            case BaitModel feed:
                _dataService.SaveFeed(feed);
                break;
            case FeedComponentModel component:
                _dataService.SaveFeedComponent(component);
                break;
            case DipModel dip:
                _dataService.SaveDip(dip);
                break;
            case LureModel lure:
                _dataService.SaveLure(lure);
                break;
        }
    }
}
