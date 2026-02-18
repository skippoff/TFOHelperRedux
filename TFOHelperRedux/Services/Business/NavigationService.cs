using System;
using System.Collections.ObjectModel;
using System.Linq;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;

namespace TFOHelperRedux.Services.Business;

/// <summary>
/// Сервис навигации между режимами приложения
/// </summary>
public class NavigationService
{
    private readonly Action<string> _onModeChanged;
    private readonly Action? _onNavigateToFish;
    private readonly Action? _onNavigateToMaps;
    private string _currentMode;

    public NavigationService(
        Action<string> onModeChanged,
        Action? onNavigateToFish = null,
        Action? onNavigateToMaps = null)
    {
        _onModeChanged = onModeChanged;
        _onNavigateToFish = onNavigateToFish;
        _onNavigateToMaps = onNavigateToMaps;
        _currentMode = DataStore.CurrentMode;
    }

    /// <summary>
    /// Текущий режим
    /// </summary>
    public string CurrentMode
    {
        get => _currentMode;
        private set
        {
            if (_currentMode != value)
            {
                _currentMode = value;
                DataStore.CurrentMode = value;
                _onModeChanged(value);
            }
        }
    }

    /// <summary>
    /// Переход к режиму рыб
    /// </summary>
    public void NavigateToFish()
    {
        CurrentMode = "Fish";
        DataStore.Selection.SelectedMap = null;
        _onNavigateToFish?.Invoke();
    }

    /// <summary>
    /// Переход к режиму карт
    /// </summary>
    public void NavigateToMaps(ObservableCollection<MapModel> maps, Action<MapModel?> setSelectedMap)
    {
        CurrentMode = "Maps";

        // Если карта ещё не выбрана — выбираем первую
        if (setSelectedMap != null && maps.Any())
        {
            setSelectedMap(maps.First());
        }

        _onNavigateToMaps?.Invoke();
    }

    /// <summary>
    /// Переход к режиму прикормок
    /// </summary>
    public void NavigateToBaits()
    {
        CurrentMode = "Baits";
    }

    /// <summary>
    /// Переход к под-режиму в прикормках
    /// </summary>
    public void NavigateToBaitsSubMode(string subMode, Action<string> setSubMode)
    {
        setSubMode(subMode);
    }

    /// <summary>
    /// Режим для топ наживок (Live/Lure)
    /// </summary>
    public void SetTopLuresMode(string mode, Action<string> setMode)
    {
        setMode(mode);
    }
}
