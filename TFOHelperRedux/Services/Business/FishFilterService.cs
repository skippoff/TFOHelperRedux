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

    public FishFilterService()
    {
        _allFishes = DataStore.Fishes;
        _filteredFishes = new ObservableCollection<FishModel>();
        _searchText = string.Empty;
    }

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
        var filtered = _allFishes.AsEnumerable();

        // Фильтр по поиску
        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            filtered = filtered.Where(f =>
                f.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
        }

        // Обновляем коллекцию
        UpdateFilteredCollection(filtered.ToList());
    }

    /// <summary>
    /// Фильтрует по карте
    /// </summary>
    public void FilterByMap(MapModel? selectedMap)
    {
        if (selectedMap == null)
        {
            // Без фильтра — все рыбы
            UpdateFilteredCollection(_allFishes.ToList());
            return;
        }

        var fishOnMap = _allFishes
            .Where(f => selectedMap.FishIDs != null && selectedMap.FishIDs.Contains(f.ID))
            .ToList();

        UpdateFilteredCollection(fishOnMap);
    }

    /// <summary>
    /// Сбрасывает текст поиска
    /// </summary>
    public void ResetSearch()
    {
        _searchText = string.Empty;
        ApplyFilter();
    }

    private void UpdateFilteredCollection(System.Collections.Generic.List<FishModel> filtered)
    {
        _filteredFishes.Clear();
        foreach (var fish in filtered)
        {
            _filteredFishes.Add(fish);
        }
    }

    public ObservableCollection<FishModel> GetFilteredFishes() => _filteredFishes;
    public ObservableCollection<FishModel> GetAllFishes() => _allFishes;
}
