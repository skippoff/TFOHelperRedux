using System;
using System.Collections.ObjectModel;
using System.Linq;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Services;

/// <summary>
/// Сервис CRUD операций для наживок (Lures), прикормок (Feeds), дипов (Dips) и компонентов (FeedComponents)
/// </summary>
public class LureService
{
    private readonly FishDataService _fishDataService;

    public LureService(FishDataService fishDataService)
    {
        _fishDataService = fishDataService;
    }

    /// <summary>
    /// Создаёт или получает наживку для редактирования
    /// </summary>
    public LureModel GetOrCreateLureForEdit(LureModel? selectedLure)
    {
        return selectedLure ?? new LureModel
        {
            ID = _fishDataService.GetNextId(DataStore.Lures),
            Name = "Новая наживка"
        };
    }

    /// <summary>
    /// Создаёт или получает прикормку для редактирования
    /// </summary>
    public BaitModel GetOrCreateFeedForEdit(BaitModel? selectedFeed)
    {
        return selectedFeed ?? new BaitModel
        {
            ID = _fishDataService.GetNextId(DataStore.Feeds),
            Name = "Новая прикормка"
        };
    }

    /// <summary>
    /// Создаёт или получает дип для редактирования
    /// </summary>
    public DipModel GetOrCreateDipForEdit(DipModel? selectedDip)
    {
        return selectedDip ?? new DipModel
        {
            ID = _fishDataService.GetNextId(DataStore.Dips),
            Name = "Новый дип"
        };
    }

    /// <summary>
    /// Создаёт или получает компонент прикормки для редактирования
    /// </summary>
    public FeedComponentModel GetOrCreateComponentForEdit(FeedComponentModel? selectedComponent)
    {
        return selectedComponent ?? new FeedComponentModel
        {
            ID = _fishDataService.GetNextId(DataStore.FeedComponents),
            Name = "Новый компонент"
        };
    }

    /// <summary>
    /// Добавляет наживку в коллекцию, если она ещё не добавлена
    /// </summary>
    public void AddLureIfNew(LureModel lure, ObservableCollection<LureModel> lures)
    {
        if (!lures.Contains(lure))
        {
            lures.Add(lure);
        }
    }

    /// <summary>
    /// Добавляет прикормку в коллекцию, если она ещё не добавлена
    /// </summary>
    public void AddFeedIfNew(BaitModel feed, ObservableCollection<BaitModel> feeds)
    {
        if (!feeds.Contains(feed))
        {
            feeds.Add(feed);
        }
    }

    /// <summary>
    /// Добавляет дип в коллекцию, если он ещё не добавлен
    /// </summary>
    public void AddDipIfNew(DipModel dip, ObservableCollection<DipModel> dips)
    {
        if (!dips.Contains(dip))
        {
            dips.Add(dip);
        }
    }

    /// <summary>
    /// Добавляет компонент прикормки в коллекцию, если он ещё не добавлен
    /// </summary>
    public void AddComponentIfNew(FeedComponentModel component, ObservableCollection<FeedComponentModel> components)
    {
        if (!components.Contains(component))
        {
            components.Add(component);
        }
    }

    /// <summary>
    /// Сохраняет изменения в зависимости от типа элемента
    /// </summary>
    public void SaveItem(IItemModel item)
    {
        switch (item)
        {
            case BaitModel feed:
                DataService.SaveFeeds(DataStore.Feeds);
                break;
            case DipModel dip:
                DataService.SaveDips(DataStore.Dips);
                break;
            case LureModel lure:
                DataService.SaveLures(DataStore.Lures);
                break;
            case FeedComponentModel component:
                DataService.SaveFeedComponents(DataStore.FeedComponents);
                break;
        }
    }
}
