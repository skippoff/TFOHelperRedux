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

        // Подписка на изменения IsSelected у наживок
        SubscribeToLureChanges();

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
        if (_selection.IsSyncingLures)
            return;

        _selection.HandleLureSelectionChanged(lure);
        LuresChanged?.Invoke();
    }

    /// <summary>
    /// Подписка на изменения IsSelected у наживок
    /// </summary>
    private void SubscribeToLureChanges()
    {
        if (DataStore.Lures == null)
            return;

        foreach (var lure in DataStore.Lures)
        {
            if (lure is INotifyPropertyChanged npc)
                npc.PropertyChanged += LureModel_PropertyChanged;
        }

        DataStore.Lures.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
                foreach (var it in e.NewItems)
                    if (it is INotifyPropertyChanged npc)
                        npc.PropertyChanged += LureModel_PropertyChanged;

            if (e.OldItems != null)
                foreach (var it in e.OldItems)
                    if (it is INotifyPropertyChanged npc)
                        npc.PropertyChanged -= LureModel_PropertyChanged;
        };
    }

    private void LureModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(LureModel.IsSelected))
            return;

        if (_selection.IsSyncingLures)
            return;

        if (sender is not LureModel lure)
            return;

        HandleLureSelectionChanged(lure);
    }
}
