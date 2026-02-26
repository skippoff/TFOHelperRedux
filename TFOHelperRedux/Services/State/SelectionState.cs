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
    /// Устанавливает выбранную рыбу и синхронизирует чекбоксы наживок
    /// </summary>
    public void SetSelectedFish(FishModel? fish, ObservableCollection<LureModel> lures)
    {
        if (SelectedFish == fish)
            return;

        SelectedFish = fish;
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
}
