using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Services.Business;

/// <summary>
/// Сервис для управления навигацией в режиме Fish.
/// Обрабатывает переключение режимов и навигацию на карты.
/// </summary>
public class FishNavigationService
{
    private readonly NavigationViewModel _navigationVM;
    private readonly FishSelectionService _selectionService;
    private readonly MapsService _mapsService;
    private readonly CatchPointsViewModel _catchPointsVM;
    private readonly FishFilterService _filterService;
    private readonly BaitsViewModel _baitsVM;

    /// <summary>
    /// Событие при изменении режима (для обновления UI)
    /// </summary>
    public event Action? ModeChanged;

    public FishNavigationService(
        NavigationViewModel navigationVM,
        FishSelectionService selectionService,
        MapsService mapsService,
        CatchPointsViewModel catchPointsVM,
        FishFilterService filterService,
        BaitsViewModel baitsVM)
    {
        _navigationVM = navigationVM;
        _selectionService = selectionService;
        _mapsService = mapsService;
        _catchPointsVM = catchPointsVM;
        _filterService = filterService;
        _baitsVM = baitsVM;

        // Подписка на изменения режимов навигации
        _navigationVM.OnModeChanged += OnModeChanged;
        _navigationVM.OnBaitsSubModeChanged += OnBaitsSubModeChanged;
    }

    /// <summary>
    /// Текущий режим
    /// </summary>
    public string CurrentMode => _navigationVM.CurrentMode;

    /// <summary>
    /// Под-режим для Baits
    /// </summary>
    public string BaitsSubMode => _navigationVM.BaitsSubMode;

    /// <summary>
    /// Установить под-режим Baits
    /// </summary>
    public void SetBaitsSubMode(string value)
    {
        _navigationVM.BaitsSubMode = value;
    }

    private void OnModeChanged()
    {
        ModeChanged?.Invoke();

        if (CurrentMode == NavigationViewModel.Modes.Fish)
        {
            _selectionService.ClearSelectedMap();
            _catchPointsVM.RefreshFilteredPoints(_selectionService.SelectedFish);
        }
        else if (CurrentMode == NavigationViewModel.Modes.Maps)
        {
            NavigateToMaps();
        }
    }

    private void OnBaitsSubModeChanged()
    {
        // Обновляем категорию в BaitsVM
        _baitsVM.SetCategory(_navigationVM.BaitsSubMode);
        ModeChanged?.Invoke();
    }

    private void NavigateToMaps()
    {
        _mapsService.NavigateToMaps(
            () =>
            {
                var filtered = _filterService.GetFilteredFishes();
                if (filtered.Cast<FishModel>().Any())
                    _selectionService.SetSelectedFish(filtered.Cast<FishModel>().First());
            },
            _catchPointsVM,
            _selectionService.SelectedFish
        );
    }
}
