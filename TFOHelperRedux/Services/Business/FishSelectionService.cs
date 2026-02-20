using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.DI;
using TFOHelperRedux.Services.State;

namespace TFOHelperRedux.Services.Business;

/// <summary>
/// Сервис для управления состоянием выбора рыбы, карты и точки лова.
/// Инкапсулирует логику установки SelectedFish/SelectedMap и уведомления об изменениях.
/// </summary>
public class FishSelectionService
{
    private readonly SelectionState _selection;
    private readonly FishFilterService _filterService;

    /// <summary>
    /// Выбранная рыба
    /// </summary>
    public FishModel? SelectedFish => _selection.SelectedFish;

    /// <summary>
    /// Выбранная карта
    /// </summary>
    public MapModel? SelectedMap => _selection.SelectedMap;

    /// <summary>
    /// Выбранная точка лова
    /// </summary>
    public CatchPointModel? SelectedCatchPoint => _selection.SelectedCatchPoint;

    /// <summary>
    /// Событие при изменении выбора рыбы
    /// </summary>
    public event Action? FishChanged;

    /// <summary>
    /// Событие при изменении выбора карты
    /// </summary>
    public event Action? MapChanged;

    public FishSelectionService()
    {
        _selection = DataStore.Selection;
        _filterService = ServiceContainer.GetService<FishFilterService>()!;

        // Подписка на изменения выбора в DataStore.Selection
        _selection.SelectionChanged += () =>
        {
            FishChanged?.Invoke();
            MapChanged?.Invoke();
        };
    }

    /// <summary>
    /// Установить выбранную рыбу
    /// </summary>
    public void SetSelectedFish(FishModel? fish)
    {
        _selection.SetSelectedFish(fish, DataStore.Lures);
        FishChanged?.Invoke();
    }

    /// <summary>
    /// Установить выбранную карту
    /// </summary>
    public void SetSelectedMap(MapModel? map)
    {
        // Игнорируем установку null при переключении между ListBox
        if (map == null)
            return;

        // Проверяем, что карта действительно изменилась
        if (_selection.SelectedMap == map)
            return;

        _selection.SetSelectedMap(map, DataStore.Fishes, _filterService.GetFilteredFishes(), DataStore.Lures);
        MapChanged?.Invoke();
        FishChanged?.Invoke();
    }

    /// <summary>
    /// Очистить выбор карты (при переключении в режим Fish)
    /// </summary>
    public void ClearSelectedMap()
    {
        _selection.SelectedMap = null;
        MapChanged?.Invoke();
    }
}
