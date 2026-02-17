using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Services;

/// <summary>
/// Сервис управления выбором рыбы и синхронизации состояния
/// </summary>
public class FishSelectionService
{
    private bool _isSyncingLures;
    private readonly Action _onSelectionChanged;
    private readonly Action _onLuresSynced;

    public FishSelectionService(Action onSelectionChanged, Action? onLuresSynced = null)
    {
        _onSelectionChanged = onSelectionChanged;
        _onLuresSynced = onLuresSynced ?? (() => { });
    }

    /// <summary>
    /// Выбранная рыба
    /// </summary>
    public FishModel? SelectedFish { get; private set; }

    /// <summary>
    /// Выбранная карта
    /// </summary>
    public MapModel? SelectedMap { get; private set; }

    /// <summary>
    /// Устанавливает выбранную рыбу и синхронизирует чекбоксы наживок
    /// </summary>
    public void SetSelectedFish(FishModel? fish, ObservableCollection<LureModel> lures)
    {
        if (SelectedFish == fish)
            return;

        SelectedFish = fish;
        DataStore.SelectedFish = fish;

        SyncLuresWithFish(fish, lures);
        _onSelectionChanged();
    }

    /// <summary>
    /// Устанавливает выбранную карту
    /// </summary>
    public void SetSelectedMap(MapModel? map, ObservableCollection<FishModel> allFishes, ObservableCollection<FishModel> filteredFishes)
    {
        if (SelectedMap == map)
            return;

        SelectedMap = map;
        DataStore.SelectedMap = map;

        // Фильтруем рыб по карте
        filteredFishes.Clear();

        if (map == null)
        {
            // Без карты — все рыбы
            foreach (var f in allFishes)
                filteredFishes.Add(f);
        }
        else
        {
            // Только рыбы на этой карте
            var fishOnMap = allFishes
                .Where(f => map.FishIDs != null && map.FishIDs.Contains(f.ID))
                .ToList();

            foreach (var fish in fishOnMap)
                filteredFishes.Add(fish);
        }

        // Выбираем первую рыбу из отфильтрованных
        if (filteredFishes.Any())
        {
            SetSelectedFish(filteredFishes.First(), DataStore.Lures);
        }
        else
        {
            SetSelectedFish(null, DataStore.Lures);
        }

        _onSelectionChanged();
    }

    /// <summary>
    /// Синхронизирует чекбоксы наживок с LureIDs выбранной рыбы
    /// </summary>
    private void SyncLuresWithFish(FishModel? fish, ObservableCollection<LureModel> lures)
    {
        if (lures == null)
            return;

        _isSyncingLures = true;
        try
        {
            var ids = fish?.LureIDs ?? Array.Empty<int>();
            foreach (var lure in lures)
            {
                lure.IsSelected = ids.Contains(lure.ID);
            }
        }
        finally
        {
            _isSyncingLures = false;
        }

        _onLuresSynced();
    }

    /// <summary>
    /// Проверяет, идёт ли синхронизация наживок
    /// </summary>
    public bool IsSyncingLures => _isSyncingLures;

    /// <summary>
    /// Обрабатывает изменение чекбокса наживки
    /// </summary>
    public void HandleLureSelectionChanged(LureModel lure, bool saveChanges = true)
    {
        if (_isSyncingLures || SelectedFish == null)
            return;

        var ids = SelectedFish.LureIDs ?? Array.Empty<int>();

        if (lure.IsSelected)
        {
            if (!ids.Contains(lure.ID))
            {
                SelectedFish.LureIDs = ids.Concat(new[] { lure.ID }).Distinct().ToArray();
                if (saveChanges)
                    DataService.SaveFishes(DataStore.Fishes);
            }
        }
        else
        {
            if (ids.Contains(lure.ID))
            {
                SelectedFish.LureIDs = ids.Where(id => id != lure.ID).ToArray();
                if (saveChanges)
                    DataService.SaveFishes(DataStore.Fishes);
            }
        }
    }
}
