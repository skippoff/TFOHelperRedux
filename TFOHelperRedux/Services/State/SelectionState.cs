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

        // Не вызываем SelectionChanged здесь — вызовем в конце один раз
        _selectedMap = map;

        // Фильтруем рыб по карте (эффективное обновление)
        UpdateFishCollection(map, allFishes, filteredFishes);

        // Выбираем первую рыбу из отфильтрованных (без лишнего уведомления)
        var firstFish = filteredFishes.Any() ? filteredFishes.First() : null;
        if (SelectedFish != firstFish)
        {
            _selectedFish = firstFish;
            SyncLuresWithFish(firstFish, lures);
        }

        // Одно уведомление об изменении выбора
        SelectionChanged?.Invoke();
    }

    /// <summary>
    /// Эффективное обновление коллекции рыб
    /// </summary>
    private static void UpdateFishCollection(
        MapModel? map,
        ObservableCollection<FishModel> allFishes,
        ObservableCollection<FishModel> filteredFishes)
    {
        IEnumerable<FishModel> fishOnMap;

        if (map == null)
        {
            fishOnMap = allFishes;
        }
        else
        {
            fishOnMap = allFishes
                .Where(f => map.FishIDs != null && map.FishIDs.Contains(f.ID));
        }

        var newSet = new HashSet<FishModel>(fishOnMap);
        var existingSet = new HashSet<FishModel>(filteredFishes);

        // Удаляем рыб, которых нет на карте
        for (int i = filteredFishes.Count - 1; i >= 0; i--)
        {
            if (!newSet.Contains(filteredFishes[i]))
                filteredFishes.RemoveAt(i);
        }

        // Добавляем новых рыб
        foreach (var fish in fishOnMap)
        {
            if (!existingSet.Contains(fish))
                filteredFishes.Add(fish);
        }
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
            // Используем HashSet для O(1) поиска вместо O(n) Contains
            var lureIdsSet = fish?.LureIDs != null && fish.LureIDs.Length > 0 
                ? new HashSet<int>(fish.LureIDs) 
                : null;
                
            var bestLureIdsSet = fish?.BestLureIDs != null && fish.BestLureIDs.Length > 0 
                ? new HashSet<int>(fish.BestLureIDs) 
                : null;

            foreach (var lure in lures)
            {
                // Быстрая проверка через HashSet.ContainsKey вместо Array.Contains
                var shouldBeSelected = lureIdsSet?.Contains(lure.ID) ?? false;
                var shouldBeBestSelected = bestLureIdsSet?.Contains(lure.ID) ?? false;

                // Устанавливаем только если значение изменилось (избегаем лишних уведомлений)
                if (lure.IsSelected != shouldBeSelected)
                    lure.IsSelected = shouldBeSelected;
                    
                if (lure.IsBestSelected != shouldBeBestSelected)
                    lure.IsBestSelected = shouldBeBestSelected;
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
