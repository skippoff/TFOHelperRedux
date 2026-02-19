using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.DI;

namespace TFOHelperRedux.Services.State;

/// <summary>
/// Централизованное хранилище состояния выбора (рыба, карта, точка лова)
/// </summary>
public class SelectionState
{
    private bool _isSyncingLures;
    private FishModel? _selectedFish;
    private MapModel? _selectedMap;
    private CatchPointModel? _selectedCatchPoint;

    /// <summary>
    /// Выбранная рыба
    /// </summary>
    public FishModel? SelectedFish
    {
        get => _selectedFish;
        set
        {
            if (_selectedFish == value)
                return;

            _selectedFish = value;
            SelectionChanged?.Invoke();
        }
    }

    /// <summary>
    /// Выбранная карта
    /// </summary>
    public MapModel? SelectedMap
    {
        get => _selectedMap;
        set
        {
            if (_selectedMap == value)
                return;

            _selectedMap = value;
            SelectionChanged?.Invoke();
        }
    }

    /// <summary>
    /// Выбранная точка лова
    /// </summary>
    public CatchPointModel? SelectedCatchPoint
    {
        get => _selectedCatchPoint;
        set
        {
            if (_selectedCatchPoint == value)
                return;

            _selectedCatchPoint = value;
            SelectionChanged?.Invoke();
        }
    }

    /// <summary>
    /// Делегат обратного вызова при изменении выбора
    /// </summary>
    public Action? SelectionChanged { get; set; }

    /// <summary>
    /// Делегат обратного вызова при синхронизации наживок
    /// </summary>
    public Action? LuresSynced { get; set; }

    /// <summary>
    /// Устанавливает выбранную рыбу и синхронизирует чекбоксы наживок
    /// </summary>
    public void SetSelectedFish(FishModel? fish, ObservableCollection<LureModel> lures)
    {
        if (SelectedFish == fish)
            return;

        SelectedFish = fish;
        SyncLuresWithFish(fish, lures);
    }

    /// <summary>
    /// Устанавливает выбранную карту и фильтрует рыб
    /// </summary>
    public void SetSelectedMap(
        MapModel? map,
        ObservableCollection<FishModel> allFishes,
        ObservableCollection<FishModel> filteredFishes,
        ObservableCollection<LureModel> lures)
    {
        if (SelectedMap == map)
            return;

        SelectedMap = map;

        // Фильтруем рыб по карте
        filteredFishes.Clear();

        if (map == null)
        {
            foreach (var f in allFishes)
                filteredFishes.Add(f);
        }
        else
        {
            var fishOnMap = allFishes
                .Where(f => map.FishIDs != null && map.FishIDs.Contains(f.ID))
                .ToList();

            foreach (var fish in fishOnMap)
                filteredFishes.Add(fish);
        }

        // Выбираем первую рыбу из отфильтрованных
        SetSelectedFish(filteredFishes.Any() ? filteredFishes.First() : null, lures);
    }

    /// <summary>
    /// Синхронизирует чекбоксы наживок с LureIDs и BestLureIDs выбранной рыбы
    /// </summary>
    private void SyncLuresWithFish(FishModel? fish, ObservableCollection<LureModel> lures)
    {
        if (lures == null)
            return;

        _isSyncingLures = true;
        try
        {
            var lureIds = fish?.LureIDs ?? Array.Empty<int>();
            var bestLureIds = fish?.BestLureIDs ?? Array.Empty<int>();
            
            foreach (var lure in lures)
            {
                lure.IsSelected = lureIds.Contains(lure.ID);
                lure.IsBestSelected = bestLureIds.Contains(lure.ID);
            }
        }
        finally
        {
            _isSyncingLures = false;
        }

        LuresSynced?.Invoke();
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

    /// <summary>
    /// Обрабатывает изменение чекбокса лучшей наживки
    /// </summary>
    public void HandleBestLureSelectionChanged(LureModel lure, bool saveChanges = true)
    {
        if (_isSyncingLures || SelectedFish == null)
            return;

        var ids = SelectedFish.BestLureIDs ?? Array.Empty<int>();

        if (lure.IsBestSelected)
        {
            if (!ids.Contains(lure.ID))
            {
                SelectedFish.BestLureIDs = ids.Concat(new[] { lure.ID }).Distinct().ToArray();
                if (saveChanges)
                    DataService.SaveFishes(DataStore.Fishes);
            }
        }
        else
        {
            if (ids.Contains(lure.ID))
            {
                SelectedFish.BestLureIDs = ids.Where(id => id != lure.ID).ToArray();
                if (saveChanges)
                    DataService.SaveFishes(DataStore.Fishes);
            }
        }
    }
}
