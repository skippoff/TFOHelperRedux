using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;

namespace TFOHelperRedux.Services.Business;

/// <summary>
/// Сервис для управления представлением карт с группировкой
/// </summary>
public class MapListViewService
{
    private ICollectionView? _allMapsView;
    private ObservableCollection<MapModel>? _cachedMaps;

    /// <summary>
    /// Получение представления для списка карт с группировкой
    /// </summary>
    public ICollectionView GetAllMapsView(ObservableCollection<MapModel> maps)
    {
        // Кэшируем коллекцию, чтобы избежать пересоздания view
        if (_cachedMaps == null)
        {
            _cachedMaps = new ObservableCollection<MapModel>(maps);
            SortMaps(_cachedMaps);
        }

        if (_allMapsView == null)
        {
            _allMapsView = CollectionViewSource.GetDefaultView(_cachedMaps);

            // Настраиваем группировку только один раз
            if (_allMapsView.GroupDescriptions?.Count == 0)
            {
                _allMapsView.GroupDescriptions?.Add(new PropertyGroupDescription("DLC"));
            }
        }

        return _allMapsView;
    }

    /// <summary>
    /// Сортировка карт
    /// </summary>
    private static void SortMaps(ObservableCollection<MapModel> maps)
    {
        var sorted = maps
            .OrderBy(m => m.DLC)
            .ThenBy(m => m.Level)
            .ThenBy(m => m.Name)
            .ToList();

        maps.Clear();
        foreach (var map in sorted)
            maps.Add(map);
    }
}
