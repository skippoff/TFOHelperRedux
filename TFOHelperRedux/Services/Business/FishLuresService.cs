using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.State;

namespace TFOHelperRedux.Services.Business;

/// <summary>
/// Сервис для управления привязкой наживок к рыбам.
/// Обрабатывает изменения IsSelected у наживок и синхронизирует с DataStore.Selection.
/// </summary>
public class FishLuresService
{
    private readonly SelectionState _selection;
    private readonly LureBindingService _lureBindingService;

    /// <summary>
    /// Событие при изменении выбора наживки (для обновления UI)
    /// </summary>
    public event Action? LuresChanged;

    public FishLuresService(LureBindingService lureBindingService)
    {
        _selection = DataStore.Selection;
        _lureBindingService = lureBindingService;

        // Подписка на изменения IsSelected у наживок ОТКЛЮЧЕНА
        // Теперь наживки работают через CatchPoint.LureIDs и CatchPoint.BestLureIDs
        // SubscribeToLureChanges();

        // Подписка на событие LuresSynced из SelectionState
        _selection.LuresSynced += () =>
        {
            LuresChanged?.Invoke();
        };
    }

    /// <summary>
    /// Обработать изменение выбора наживки (вызов из UI)
    /// </summary>
    public void HandleLureSelectionChanged(LureModel lure)
    {
        // Наживки теперь работают через CatchPoint.LureIDs
        // Этот метод оставлен для обратной совместимости
    }

    /// <summary>
    /// Обработать изменение выбора лучшей наживки (вызов из UI)
    /// </summary>
    public void HandleBestLureSelectionChanged(LureModel lure)
    {
        // Наживки теперь работают через CatchPoint.BestLureIDs
        // Этот метод оставлен для обратной совместимости
    }
}
