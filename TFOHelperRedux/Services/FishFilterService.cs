using System;
using System.Collections.ObjectModel;
using System.Linq;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Services;

/// <summary>
/// Сервис фильтрации и поиска рыб
/// </summary>
public class FishFilterService
{
    private readonly ObservableCollection<FishModel> _allFishes;
    private readonly ObservableCollection<FishModel> _filteredFishes;
    private string _searchText;

    public FishFilterService(
        ObservableCollection<FishModel> allFishes,
        ObservableCollection<FishModel> filteredFishes)
    {
        _allFishes = allFishes;
        _filteredFishes = filteredFishes;
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
}
