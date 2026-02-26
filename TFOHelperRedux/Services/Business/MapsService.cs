using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Services.Business
{
    /// <summary>
    /// Сервис для управления картами: фильтрация, обновление данных для рыб, навигация
    /// </summary>
    public class MapsService
    {
        private readonly IDataLoadSaveService _loadSaveService;
        private readonly Action? _onMapsChanged;
        private readonly Action? _onSelectedMapChanged;
        private readonly Action? _onSelectedLevelFilterChanged;

        public MapsService(
            IDataLoadSaveService loadSaveService,
            ObservableCollection<MapModel> maps,
            Action? onMapsChanged = null,
            Action? onSelectedMapChanged = null,
            Action? onSelectedLevelFilterChanged = null)
        {
            _loadSaveService = loadSaveService;
            Maps = maps;
            _onMapsChanged = onMapsChanged;
            _onSelectedMapChanged = onSelectedMapChanged;
            _onSelectedLevelFilterChanged = onSelectedLevelFilterChanged;
        }

        #region Коллекции карт

        public ObservableCollection<MapModel> MapsForFish { get; } = new();
        public ObservableCollection<MapModel> NonDlcMaps { get; } = new();
        public ObservableCollection<MapModel> DlcMaps { get; } = new();
        public ObservableCollection<int> MapLevels { get; } = new();

        #endregion

        #region Свойства

        private int _selectedLevelFilter;
        public int SelectedLevelFilter
        {
            get => _selectedLevelFilter;
            set
            {
                if (_selectedLevelFilter != value)
                {
                    _selectedLevelFilter = value;
                    _onSelectedLevelFilterChanged?.Invoke();
                    UpdateMapFilters();
                }
            }
        }

        private MapModel? _selectedMap;
        public MapModel? SelectedMap
        {
            get => _selectedMap;
            set
            {
                if (_selectedMap != value)
                {
                    _selectedMap = value;
                    _onSelectedMapChanged?.Invoke();
                }
            }
        }

        public ObservableCollection<MapModel> Maps { get; set; } = null!;

        #endregion

        #region Методы обновления карт для рыбы

        private readonly Dictionary<int, BitmapImage?> _fishImageCache = new();

        public void UpdateMapsForFish(FishModel? selectedFish)
        {
            MapsForFish.Clear();

            if (selectedFish == null)
                return;

            var maps = DataStore.Maps
                .Where(m => m.FishIDs != null && m.FishIDs.Contains(selectedFish.ID))
                .OrderBy(m => m.Name);

            foreach (var map in maps)
                MapsForFish.Add(map);
        }

        public BitmapImage? GetFishImage(int? fishId)
        {
            if (fishId == null)
                return null;

            // Проверяем кэш
            if (_fishImageCache.TryGetValue(fishId.Value, out var cachedImage))
                return cachedImage;

            // Загружаем изображение
            var imgPath = _loadSaveService.GetFishImagePath(fishId.Value);
            if (File.Exists(imgPath))
            {
                var bitmap = new BitmapImage(new Uri(imgPath));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 144; // Оптимизация памяти
                _fishImageCache[fishId.Value] = bitmap;
                return bitmap;
            }

            _fishImageCache[fishId.Value] = null;
            return null;
        }

        #endregion

        #region Методы фильтрации карт

        public void InitializeMapFilters()
        {
            MapLevels.Clear();

            if (Maps != null)
            {
                foreach (var lvl in Maps.Select(m => m.Level).Distinct().OrderBy(l => l))
                    MapLevels.Add(lvl);

                if (MapLevels.Any())
                    SelectedLevelFilter = MapLevels.Max();
            }
        }

        public void UpdateMapFilters()
        {
            NonDlcMaps.Clear();
            DlcMaps.Clear();

            if (Maps == null || Maps.Count == 0)
                return;

            var nonDlc = Maps.Where(m => !m.DLC);
            var dlc = Maps.Where(m => m.DLC);

            if (SelectedLevelFilter > 0)
                nonDlc = nonDlc.Where(m => m.Level <= SelectedLevelFilter);

            foreach (var map in nonDlc.OrderBy(m => m.Level).ThenBy(m => m.Name))
                NonDlcMaps.Add(map);

            foreach (var map in dlc.OrderBy(m => m.Level).ThenBy(m => m.Name))
                DlcMaps.Add(map);
        }

        #endregion

        #region Методы навигации

        public void NavigateToMaps(
            Action selectFirstFish,
            CatchPointsViewModel catchPointsVm,
            FishModel? selectedFish)
        {
            if (SelectedMap == null && Maps != null && Maps.Any())
            {
                SelectedMap = Maps.First();
            }

            if (selectedFish == null)
            {
                selectFirstFish();
            }

            catchPointsVm.RefreshFilteredPoints(selectedFish);
        }

        #endregion

        #region Вспомогательные методы

        public void SelectFirstDlcMapIfNull()
        {
            if (SelectedMap == null && NonDlcMaps.Any())
                SelectedMap = NonDlcMaps.First();
            else if (SelectedMap == null && DlcMaps.Any())
                SelectedMap = DlcMaps.First();
        }

        #endregion
    }
}
