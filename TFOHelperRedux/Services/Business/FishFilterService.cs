using System;
using System.Collections.ObjectModel;
using System.Linq;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;

namespace TFOHelperRedux.Services.Business;

/// <summary>
/// Сервис фильтрации и поиска рыб
/// </summary>
public class FishFilterService
{
    private readonly ObservableCollection<FishModel> _allFishes;
    private readonly ObservableCollection<FishModel> _filteredFishes;
    private string _searchText;
    private MapModel? _selectedMap;

    public FishFilterService()
    {
        _allFishes = DataStore.Fishes;
        _filteredFishes = new ObservableCollection<FishModel>(_allFishes);
        _searchText = string.Empty;
    }

    /// <summary>
    /// Отфильтрованная коллекция рыб
    /// </summary>
    public ObservableCollection<FishModel> FilteredFishes => _filteredFishes;

    /// <summary>
    /// Текст поиска
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                ApplyFilter();
            }
        }
    }

    /// <summary>
    /// Применяет фильтр по поиску
    /// </summary>
    public void ApplyFilter()
    {
        UpdateFilteredCollection();
    }

    /// <summary>
    /// Обновляет отфильтрованную коллекцию
    /// </summary>
    private void UpdateFilteredCollection()
    {
        _filteredFishes.Clear();

        // 1. Сначала фильтр по карте (рыбы на водоёме)
        IEnumerable<FishModel> filtered = _allFishes.AsEnumerable();
        
        if (_selectedMap != null && _selectedMap.FishIDs != null)
        {
            filtered = filtered.Where(f => _selectedMap.FishIDs.Contains(f.ID));
        }

        // 2. Затем поиск по названию (только среди рыб на водоёме)
        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            filtered = filtered.Where(f =>
                f.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var fish in filtered)
        {
            _filteredFishes.Add(fish);
        }
    }

    /// <summary>
    /// Устанавливает выбранную карту и обновляет фильтр
    /// </summary>
    public void SetSelectedMap(MapModel? map)
    {
        _selectedMap = map;
        ApplyFilter();
    }

    public ObservableCollection<FishModel> GetFilteredFishes() => _filteredFishes;
}
