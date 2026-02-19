using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;

namespace TFOHelperRedux.Services.Business;

/// <summary>
/// Сервис для управления представлением карт с группировкой и фильтрацией
/// </summary>
public class MapListViewService
{
    private ICollectionView? _allMapsView;

    /// <summary>
    /// Получение представления для списка карт с группировкой
    /// </summary>
    public ICollectionView GetAllMapsView(ObservableCollection<MapModel> maps, int selectedLevelFilter)
    {
        if (_allMapsView == null)
        {
            _allMapsView = CollectionViewSource.GetDefaultView(maps);
            
            // Настраиваем группировку только один раз
            if (_allMapsView.GroupDescriptions?.Count == 0)
            {
                _allMapsView.GroupDescriptions?.Add(new PropertyGroupDescription("DLC"));
            }
            
            // Настраиваем сортировку только один раз
            if (_allMapsView.SortDescriptions?.Count == 0)
            {
                _allMapsView.SortDescriptions?.Add(new SortDescription("DLC", ListSortDirection.Ascending));
                _allMapsView.SortDescriptions?.Add(new SortDescription("Level", ListSortDirection.Ascending));
                _allMapsView.SortDescriptions?.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
        }
        
        // Применяем фильтрацию
        _allMapsView.Filter = m =>
        {
            if (m is not MapModel map)
                return false;
            if (selectedLevelFilter <= 0)
                return true;
            // DLC карты показываем всегда, обычные фильтруем по уровню
            return map.DLC || map.Level <= selectedLevelFilter;
        };
        
        return _allMapsView;
    }

    /// <summary>
    /// Обновление фильтра
    /// </summary>
    public void RefreshFilter(int selectedLevelFilter)
    {
        if (_allMapsView != null)
        {
            _allMapsView.Filter = m =>
            {
                if (m is not MapModel map)
                    return false;
                if (selectedLevelFilter <= 0)
                    return true;
                return map.DLC || map.Level <= selectedLevelFilter;
            };
            _allMapsView.Refresh();
        }
    }
}
