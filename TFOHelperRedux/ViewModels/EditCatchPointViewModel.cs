using System.Collections.ObjectModel;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;

namespace TFOHelperRedux.ViewModels
{
    public class EditCatchPointViewModel : BaseViewModel

    {
    public CatchPointModel CatchPoint { get; }

    // Коллекции для привязки (галочки)
    public ObservableCollection<SelectableItem<FishModel>> AvailableFishes { get; set; }
    public ObservableCollection<SelectableItem<LureModel>> Baits { get; set; }
    public ObservableCollection<SelectableItem<BaitModel>> Feeds { get; set; }
    public ObservableCollection<SelectableItem<DipModel>> Dips { get; set; }

    public EditCatchPointViewModel(CatchPointModel point)
    {
        CatchPoint = point ?? new CatchPointModel();

        // гарантируем, что поля не null
        CatchPoint.FishIDs ??= Array.Empty<int>();
        CatchPoint.LureIDs ??= Array.Empty<int>();
        CatchPoint.FeedIDs ??= Array.Empty<int>();
        CatchPoint.DipsIDs ??= Array.Empty<int>();

        // 🐟 данные берём из DataStore, как было раньше
        var fishes = DataStore.Fishes;
        var lures = DataStore.Lures;
        var feeds = DataStore.Feeds;
        var dips = DataStore.Dips;

        // формируем коллекции с галочками
        AvailableFishes = new ObservableCollection<SelectableItem<FishModel>>(
            fishes.Select(f => new SelectableItem<FishModel>(f, CatchPoint.FishIDs.Contains(f.ID))));

        Baits = new ObservableCollection<SelectableItem<LureModel>>(
            lures.Select(b => new SelectableItem<LureModel>(b, CatchPoint.LureIDs.Contains(b.ID))));

        Feeds = new ObservableCollection<SelectableItem<BaitModel>>(
            feeds.Select(f => new SelectableItem<BaitModel>(f, CatchPoint.FeedIDs.Contains(f.ID))));

        Dips = new ObservableCollection<SelectableItem<DipModel>>(
            dips.Select(d => new SelectableItem<DipModel>(d, CatchPoint.DipsIDs.Contains(d.ID))));
    }

    public void ApplyChanges()
    {
        // обновляем ID-массивы по состоянию галочек
        CatchPoint.FishIDs = AvailableFishes.Where(f => f.IsSelected).Select(f => f.Value.ID).ToArray();
        CatchPoint.LureIDs = Baits.Where(b => b.IsSelected).Select(b => b.Value.ID).ToArray();
        CatchPoint.FeedIDs = Feeds.Where(f => f.IsSelected).Select(f => f.Value.ID).ToArray();
        CatchPoint.DipsIDs = Dips.Where(d => d.IsSelected).Select(d => d.Value.ID).ToArray();
    }
    }
}
