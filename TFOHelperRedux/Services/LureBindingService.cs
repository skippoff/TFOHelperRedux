using System;
using System.Collections.Generic;
using System.Linq;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Services;

/// <summary>
/// Сервис привязки наживок к рыбам (BestLureIDs)
/// </summary>
public class LureBindingService
{
    private readonly IDataLoadSaveService _loadSaveService;

    public LureBindingService(IDataLoadSaveService loadSaveService)
    {
        _loadSaveService = loadSaveService;
    }

    /// <summary>
    /// Добавляет наживку в список лучших для рыбы
    /// </summary>
    public ServiceResult AttachLureToFish(LureModel lure, FishModel? selectedFish)
    {
        if (selectedFish == null)
        {
            return ServiceResult.Failure("Сначала выберите рыбу в правой панели.");
        }

        selectedFish.BestLureIDs ??= Array.Empty<int>();

        if (selectedFish.BestLureIDs.Contains(lure.ID))
        {
            return ServiceResult.Failure(
                $"Наживка «{lure.Name}» уже есть в списке лучших для рыбы «{selectedFish.Name}».");
        }

        selectedFish.BestLureIDs = selectedFish.BestLureIDs
            .Concat(new[] { lure.ID })
            .Distinct()
            .ToArray();

        _loadSaveService.SaveFishes(DataStore.Fishes);

        return ServiceResult.Success(
            $"Наживка «{lure.Name}» добавлена в лучшие для рыбы «{selectedFish.Name}».");
    }

    /// <summary>
    /// Удаляет наживку из списка лучших для рыбы
    /// </summary>
    public ServiceResult DetachLureFromFish(LureModel lure, FishModel? selectedFish)
    {
        if (selectedFish == null)
        {
            return ServiceResult.Failure("Сначала выберите рыбу в правой панели.");
        }

        if (selectedFish.BestLureIDs == null || selectedFish.BestLureIDs.Length == 0)
        {
            return ServiceResult.Failure($"У рыбы «{selectedFish.Name}» ещё нет лучших наживок.");
        }

        if (!selectedFish.BestLureIDs.Contains(lure.ID))
        {
            return ServiceResult.Failure(
                $"Наживки «{lure.Name}» нет в списке лучших для рыбы «{selectedFish.Name}».");
        }

        selectedFish.BestLureIDs = selectedFish.BestLureIDs
            .Where(id => id != lure.ID)
            .ToArray();

        _loadSaveService.SaveFishes(DataStore.Fishes);

        return ServiceResult.Success(
            $"Наживка «{lure.Name}» убрана из лучших для рыбы «{selectedFish.Name}».");
    }

    /// <summary>
    /// Удаляет рецепт из списка для рыбы
    /// </summary>
    public ServiceResult RemoveRecipeFromFish(BaitRecipeModel recipe, FishModel? selectedFish)
    {
        if (selectedFish == null)
        {
            return ServiceResult.Failure("Сначала выберите рыбу.");
        }

        if (selectedFish.RecipeIDs == null || selectedFish.RecipeIDs.Length == 0)
        {
            return ServiceResult.Failure($"У рыбы «{selectedFish.Name}» ещё нет рецептов.");
        }

        if (!selectedFish.RecipeIDs.Contains(recipe.ID))
        {
            return ServiceResult.Failure(
                $"Рецепта «{recipe.Name}» нет в списке для рыбы «{selectedFish.Name}».");
        }

        selectedFish.RecipeIDs = selectedFish.RecipeIDs
            .Where(id => id != recipe.ID)
            .ToArray();

        _loadSaveService.SaveFishes(DataStore.Fishes);

        return ServiceResult.Success(
            $"Рецепт «{recipe.Name}» убран из списка для рыбы «{selectedFish.Name}».");
    }

    /// <summary>
    /// Получает лучшие магазинные наживки для рыбы
    /// </summary>
    public IEnumerable<LureModel> GetTopLuresForFish(FishModel? fish)
    {
        if (fish == null || fish.BestLureIDs == null)
            return Enumerable.Empty<LureModel>();

        return DataStore.Lures.Where(l => fish.BestLureIDs.Contains(l.ID));
    }

    /// <summary>
    /// Получает лучшие крафтовые рецепты для рыбы
    /// </summary>
    public IEnumerable<BaitRecipeModel> GetTopRecipesForFish(FishModel? fish)
    {
        if (fish == null || fish.BestRecipeIDs == null)
            return Enumerable.Empty<BaitRecipeModel>();

        return DataStore.BaitRecipes.Where(r => fish.BestRecipeIDs.Contains(r.ID));
    }
}
