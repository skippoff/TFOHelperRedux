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
    /// Фильтрует по карте
    /// </summary>
    public void FilterByMap(MapModel? selectedMap)
    {
        _selectedMap = selectedMap;
        UpdateFilteredCollection();
    }

    /// <summary>
    /// Сбрасывает текст поиска
    /// </summary>
    public void ResetSearch()
    {
        _searchText = string.Empty;
        UpdateFilteredCollection();
    }

    /// <summary>
    /// Обновляет отфильтрованную коллекцию
    /// </summary>
    private void UpdateFilteredCollection()
    {
        _filteredFishes.Clear();

        var filtered = _allFishes.AsEnumerable();

        // Фильтр по поиску
        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            filtered = filtered.Where(f =>
                f.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
        }

        // Фильтр по карте
        if (_selectedMap != null && _selectedMap.FishIDs != null)
        {
            filtered = filtered.Where(f => _selectedMap.FishIDs.Contains(f.ID));
        }

        foreach (var fish in filtered)
        {
            _filteredFishes.Add(fish);
        }
    }

    public ObservableCollection<FishModel> GetFilteredFishes() => _filteredFishes;
    public ObservableCollection<FishModel> GetAllFishes() => _allFishes;
}
