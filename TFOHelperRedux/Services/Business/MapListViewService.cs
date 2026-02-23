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
    private ObservableCollection<MapModel>? _cachedMaps;

    /// <summary>
    /// Получение представления для списка карт с группировкой
    /// </summary>
    public ICollectionView GetAllMapsView(ObservableCollection<MapModel> maps, int selectedLevelFilter)
    {
        // Кэшируем коллекцию, чтобы избежать пересоздания view
        if (_cachedMaps == null)
        {
            _cachedMaps = new ObservableCollection<MapModel>(maps);
            SortMaps(_cachedMaps, selectedLevelFilter);
        }

        if (_allMapsView == null)
        {
            _allMapsView = CollectionViewSource.GetDefaultView(_cachedMaps);
            _currentLevelFilter = selectedLevelFilter;

            // Настраиваем группировку только один раз
            if (_allMapsView.GroupDescriptions?.Count == 0)
            {
                _allMapsView.GroupDescriptions?.Add(new PropertyGroupDescription("DLC"));
            }

            // Создаём и применяем фильтр один раз
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
        // Не обновляем, если фильтр не изменился или view ещё не создан
        if (_allMapsView == null || _currentLevelFilter == selectedLevelFilter)
            return;

        _currentLevelFilter = selectedLevelFilter;

        // Пересоздаём фильтр с новым значением
        _currentFilter = CreateFilter(selectedLevelFilter);
        _allMapsView.Filter = _currentFilter;

        // Сортируем заново
        if (_cachedMaps != null)
        {
            SortMaps(_cachedMaps, selectedLevelFilter);
        }

        // Не вызываем Refresh() явно — это приводит к сбросу скролла
        // ICollectionView применит фильтр автоматически
    }

    /// <summary>
    /// Сортировка карт
    /// </summary>
    private static void SortMaps(ObservableCollection<MapModel> maps, int selectedLevelFilter)
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
