using System.Collections.ObjectModel;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Services;

/// <summary>
/// Интерфейс для загрузки и сохранения данных
/// </summary>
public interface IDataLoadSaveService
{
    void LoadAll();
    void SaveAll();

    ObservableCollection<FishModel> LoadFishes();
    ObservableCollection<MapModel> LoadMaps();
    ObservableCollection<BaitModel> LoadFeeds();
    ObservableCollection<FeedComponentModel> LoadFeedComponents();
    ObservableCollection<BaitRecipeModel> LoadBaitRecipes();
    ObservableCollection<DipModel> LoadDips();
    ObservableCollection<LureModel> LoadLures();
    ObservableCollection<TagModel> LoadTags();

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
