using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Services.Data;

/// <summary>
/// Интерфейс для загрузки и сохранения данных
/// </summary>
public interface IDataLoadSaveService
{
    void LoadAll();
    void SaveAll();
    Task LoadAllAsync();
    Task SaveAllAsync();

    // Конкретные методы загрузки
    ObservableCollection<FishModel> LoadFishes();
    Task<ObservableCollection<FishModel>> LoadFishesAsync();
    ObservableCollection<MapModel> LoadMaps();
    Task<ObservableCollection<MapModel>> LoadMapsAsync();
    ObservableCollection<BaitModel> LoadFeeds();
    Task<ObservableCollection<BaitModel>> LoadFeedsAsync();
    ObservableCollection<FeedComponentModel> LoadFeedComponents();
    Task<ObservableCollection<FeedComponentModel>> LoadFeedComponentsAsync();
    ObservableCollection<BaitRecipeModel> LoadBaitRecipes();
    Task<ObservableCollection<BaitRecipeModel>> LoadBaitRecipesAsync();
    ObservableCollection<DipModel> LoadDips();
    Task<ObservableCollection<DipModel>> LoadDipsAsync();
    ObservableCollection<LureModel> LoadLures();
    Task<ObservableCollection<LureModel>> LoadLuresAsync();

    // Конкретные методы сохранения
    void SaveFishes(ObservableCollection<FishModel> fishes);
    void SaveFeeds(ObservableCollection<BaitModel> feeds);
    void SaveFeedComponents(ObservableCollection<FeedComponentModel> components);
    void SaveBaitRecipes(ObservableCollection<BaitRecipeModel> recipes);
    void SaveDips(ObservableCollection<DipModel> dips);
    void SaveLures(ObservableCollection<LureModel> lures);
    void SaveMaps(ObservableCollection<MapModel> maps);
    void SaveCatchPoints(string path, ObservableCollection<CatchPointModel> catchPoints);

    string GetFishImagePath(int id);
}
