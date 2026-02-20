using System;
using System.Windows;
using System.Windows.Input;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.DI;
using TFOHelperRedux.Services.UI;

namespace TFOHelperRedux.Services.Business;

/// <summary>
/// Сервис для команд привязки наживок к рыбам.
/// </summary>
public class FishLuresCommandsService
{
    private readonly FishSelectionService _selectionService;
    private readonly LureBindingService _lureBindingService;

    public FishLuresCommandsService(
        FishSelectionService selectionService,
        LureBindingService lureBindingService)
    {
        _selectionService = selectionService;
        _lureBindingService = lureBindingService;
    }

    /// <summary>
    /// Команда привязки наживки к рыбе
    /// </summary>
    public ICommand AttachLureToFishCmd => new RelayCommand(AttachLureToFish);

    /// <summary>
    /// Команда отвязки наживки от рыбы
    /// </summary>
    public ICommand DetachLureFromFishCmd => new RelayCommand(DetachLureFromFish);

    /// <summary>
    /// Команда удаления рецепта навсегда
    /// </summary>
    public ICommand DeleteRecipeForeverCmd => new RelayCommand(DeleteRecipeForever);

    private void AttachLureToFish(object? parameter)
    {
        if (parameter is not LureModel lure)
            return;

        var result = _lureBindingService.AttachLureToFish(lure, _selectionService.SelectedFish);
        result.ShowMessageBox(ServiceContainer.GetService<IUIService>());
    }

    private void DetachLureFromFish(object? parameter)
    {
        if (parameter is not LureModel lure)
            return;

        var result = _lureBindingService.DetachLureFromFish(lure, _selectionService.SelectedFish);
        result.ShowMessageBox(ServiceContainer.GetService<IUIService>());
    }

    private void DeleteRecipeForever(object? parameter)
    {
        if (parameter is not BaitRecipeModel recipe)
            return;

        var uiService = ServiceContainer.GetService<IUIService>();
        var result = uiService.ShowMessageBox(
            $"Удалить рецепт \"{recipe.Name}\" только для текущей рыбы?",
            "Удаление рецепта для рыбы",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        var bindResult = _lureBindingService.RemoveRecipeFromFish(recipe, _selectionService.SelectedFish);
        bindResult.ShowMessageBox(uiService);
    }
}
