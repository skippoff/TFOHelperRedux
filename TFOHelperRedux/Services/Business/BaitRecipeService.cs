using System;
using System.Collections.ObjectModel;
using System.Linq;
using Serilog;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;

namespace TFOHelperRedux.Services.Business;

/// <summary>
/// Сервис CRUD операций для рецептов прикормок (BaitRecipes)
/// </summary>
public class BaitRecipeService
{
    private static ILogger? _log;
    private static ILogger Log => _log ??= Serilog.Log.ForContext<BaitRecipeService>();
    
    private readonly IDataLoadSaveService _loadSaveService;

    public BaitRecipeService(IDataLoadSaveService? loadSaveService = null)
    {
        _loadSaveService = loadSaveService ?? new DataLoadSaveService();
    }

    /// <summary>
    /// Создаёт новый рецепт по умолчанию
    /// </summary>
    public BaitRecipeModel CreateNewRecipe()
    {
        return new BaitRecipeModel
        {
            Name = "Новый рецепт",
            FeedIDs = Array.Empty<int>(),
            LureIDs = Array.Empty<int>(),
            DipIDs = Array.Empty<int>(),
            ComponentIDs = Array.Empty<int>()
        };
    }

    /// <summary>
    /// Добавляет элемент в рецепт
    /// </summary>
    public void AddItemToRecipe(BaitRecipeModel recipe, IItemModel item)
    {
        if (recipe == null || item == null)
            return;

        switch (item)
        {
            case BaitModel feed:
                if (!recipe.FeedIDs.Contains(feed.ID))
                    recipe.FeedIDs = recipe.FeedIDs.Append(feed.ID).ToArray();
                break;

            case LureModel lure:
                if (!recipe.LureIDs.Contains(lure.ID))
                    recipe.LureIDs = recipe.LureIDs.Append(lure.ID).ToArray();
                break;

            case DipModel dip:
                if (!recipe.DipIDs.Contains(dip.ID))
                    recipe.DipIDs = recipe.DipIDs.Append(dip.ID).ToArray();
                break;

            case FeedComponentModel comp:
                if (!recipe.ComponentIDs.Contains(comp.ID))
                    recipe.ComponentIDs = recipe.ComponentIDs.Append(comp.ID).ToArray();
                break;
        }
    }

    /// <summary>
    /// Очищает рецепт от всех элементов
    /// </summary>
    public void ClearRecipe(BaitRecipeModel recipe)
    {
        if (recipe == null)
            return;

        recipe.FeedIDs = Array.Empty<int>();
        recipe.LureIDs = Array.Empty<int>();
        recipe.DipIDs = Array.Empty<int>();
        recipe.ComponentIDs = Array.Empty<int>();
    }

    /// <summary>
    /// Сохраняет рецепт в коллекцию
    /// </summary>
    public void SaveRecipe(BaitRecipeModel recipe, ObservableCollection<BaitRecipeModel> allRecipes)
    {
        if (recipe == null)
        {
            Log.Warning("Попытка сохранить null рецепт");
            throw new ArgumentNullException(nameof(recipe), "Рецепт не может быть null");
        }

        if (allRecipes == null)
        {
            Log.Warning("Попытка сохранить рецепт в null коллекцию");
            throw new ArgumentNullException(nameof(allRecipes), "Коллекция рецептов не может быть null");
        }

        try
        {
            recipe.DateEdited = DateTime.Now;

            // если рецепт ещё не в коллекции – присваиваем новый ID и добавляем
            if (!allRecipes.Contains(recipe))
            {
                // Проверяем каждый рецепт в коллекции на null
                foreach (var r in allRecipes)
                {
                    if (r == null)
                    {
                        Log.Error("Обнаружен null рецепт в коллекции!");
                        throw new InvalidOperationException("Коллекция рецептов содержит null элемент");
                    }
                }

                int newId = allRecipes.Any() ? allRecipes.Max(r => r.ID) + 1 : 0;
                recipe.ID = newId;
                allRecipes.Add(recipe);
            }

            _loadSaveService?.SaveBaitRecipes(allRecipes);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка сохранения рецепта: RecipeName={RecipeName}, ID={RecipeId}", recipe.Name, recipe.ID);
            throw;
        }
    }

    /// <summary>
    /// Удаляет рецепт из коллекции
    /// </summary>
    public void DeleteRecipe(BaitRecipeModel recipe)
    {
        if (recipe == null || DataStore.BaitRecipes == null)
            return;

        DataStore.BaitRecipes.Remove(recipe);
        _loadSaveService.SaveBaitRecipes(DataStore.BaitRecipes);
    }

    /// <summary>
    /// Получает коллекцию видимых рецептов (не скрытых)
    /// </summary>
    public ObservableCollection<BaitRecipeModel> GetVisibleRecipes()
    {
        var recipes = new ObservableCollection<BaitRecipeModel>();

        if (DataStore.BaitRecipes == null)
            return recipes;

        foreach (var r in DataStore.BaitRecipes.Where(r => !r.IsHidden))
            recipes.Add(r);

        return recipes;
    }

    /// <summary>
    /// Нормализует ID рецептов (переиндексация при дубликатах)
    /// </summary>
    public void NormalizeRecipeIds(ObservableCollection<BaitRecipeModel> recipes)
    {
        if (recipes == null || recipes.Count == 0)
            return;

        var distinctCount = recipes.Select(r => r.ID).Distinct().Count();
        if (distinctCount != recipes.Count)
        {
            int id = 0;
            foreach (var r in recipes)
            {
                r.ID = id++;
            }

            _loadSaveService.SaveBaitRecipes(recipes);
        }
    }
}
