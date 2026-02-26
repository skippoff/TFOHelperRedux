using System;
using System.Collections.Generic;
using System.Linq;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.DI;

namespace TFOHelperRedux.Services.Business;

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
    /// Добавляет наживку в список лучших для точки лова
    /// </summary>
    public ServiceResult AttachLureToFish(LureModel lure, FishModel? selectedFish)
    {
        // Примечание: BestLureIDs теперь хранятся в CatchPointModel, а не в FishModel
        // Этот метод оставлен для обратной совместимости, но не выполняет никаких действий
        return ServiceResult.Failure("BestLureIDs теперь хранятся в CatchPointModel. Используйте редактирование точки лова.");
    }

    /// <summary>
    /// Удаляет наживку из списка лучших для точки лова
    /// </summary>
    public ServiceResult DetachLureFromFish(LureModel lure, FishModel? selectedFish)
    {
        // Примечание: BestLureIDs теперь хранятся в CatchPointModel, а не в FishModel
        // Этот метод оставлен для обратной совместимости, но не выполняет никаких действий
        return ServiceResult.Failure("BestLureIDs теперь хранятся в CatchPointModel. Используйте редактирование точки лова.");
    }

    /// <summary>
    /// Получает лучшие магазинные наживки для точки лова
    /// </summary>
    public IEnumerable<LureModel> GetTopLuresForFish(FishModel? fish)
    {
        // Примечание: BestLureIDs теперь хранятся в CatchPointModel, а не в FishModel
        return Enumerable.Empty<LureModel>();
    }

    /// <summary>
    /// Получает лучшие крафтовые рецепты для точки лова
    /// </summary>
    public IEnumerable<BaitRecipeModel> GetTopRecipesForFish(FishModel? fish)
    {
        // Примечание: BestRecipeIDs теперь хранятся в CatchPointModel, а не в FishModel
        return Enumerable.Empty<BaitRecipeModel>();
    }
}
