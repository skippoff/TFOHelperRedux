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
    private Predicate<object>? _currentFilter;
    private int _currentLevelFilter;

    /// <summary>
    /// Получение представления для списка карт с группировкой
    /// </summary>
    public ICollectionView GetAllMapsView(ObservableCollection<MapModel> maps, int selectedLevelFilter)
    {
        if (_allMapsView == null)
        {
            _allMapsView = CollectionViewSource.GetDefaultView(maps);
            _currentLevelFilter = selectedLevelFilter;

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

            // Создаём фильтр один раз
            _currentFilter = CreateFilter(selectedLevelFilter);
            _allMapsView.Filter = _currentFilter;
        }

        return _allMapsView;
    }

    /// <summary>
    /// Обновление фильтра (только при изменении уровня)
    /// </summary>
    public void RefreshFilter(int selectedLevelFilter)
    {
        // Не обновляем, если фильтр не изменился
        if (_allMapsView == null || _currentLevelFilter == selectedLevelFilter)
            return;

        _currentLevelFilter = selectedLevelFilter;

        // Создаём новый фильтр только если изменился уровень
        if (_currentFilter == null)
        {
            _currentFilter = CreateFilter(selectedLevelFilter);
            _allMapsView.Filter = _currentFilter;
        }
        else
        {
            // Переиспользуем существующий делегат, просто обновляем замыкание
            _allMapsView.Refresh();
        }
    }

    /// <summary>
    /// Создание делегата фильтра
    /// </summary>
    private Predicate<object> CreateFilter(int selectedLevelFilter)
    {
        return m =>
        {
            if (m is not MapModel map)
                return false;
            if (selectedLevelFilter <= 0)
                return true;
            // DLC карты показываем всегда, обычные фильтруем по уровню
            return map.DLC || map.Level <= selectedLevelFilter;
        };
    }
}
