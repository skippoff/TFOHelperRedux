using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Business;
using TFOHelperRedux.Services.Data;

namespace TFOHelperRedux.ViewModels
{
    public class MapsViewModel : BaseViewModel
    {
        public ObservableCollection<MapModel> Maps { get; }
        public ObservableCollection<MapModel> FilteredMaps { get; private set; }
        public ObservableCollection<FishModel> FishList { get; private set; }

        private MapModel? _selectedMap;
        public MapModel? SelectedMap
        {
            get => _selectedMap;
            set
            {
                if (_selectedMap != value)
                {
                    _selectedMap = value;
                    OnPropertyChanged(nameof(SelectedMap));
                    UpdateMapInfo();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    ApplyFilter();
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }
        private string _searchText = string.Empty;

        public string LevelText { get; private set; } = "";
        public string SizeText { get; private set; } = "";
        public string DepthText { get; private set; } = "";
        public string DlcText { get; private set; } = "";
        public BitmapImage? MapImage { get; private set; }

        public MapsViewModel()
        {
            Maps = DataStore.Maps;
            FilteredMaps = new ObservableCollection<MapModel>(Maps);
            FishList = new ObservableCollection<FishModel>();
        }

        private void ApplyFilter()
        {
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Maps
                : new ObservableCollection<MapModel>(
                    Maps.Where(m => m.Name.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)));

            FilteredMaps.Clear();
            foreach (var m in filtered)
                FilteredMaps.Add(m);
        }

        private void UpdateMapInfo()
        {
            if (SelectedMap == null)
                return;

            // 🐟 Обновляем список рыб
            FishList.Clear();
            var fishes = DataStore.Fishes
                .Where(f => SelectedMap.FishIDs.Contains(f.ID))
                .OrderBy(f => f.Name);

            foreach (var f in fishes)
                FishList.Add(f);

            // 🗺️ Загружаем изображение карты
            string mapPath = DataService.GetMapImagePath(SelectedMap.ID);
            if (System.IO.File.Exists(mapPath))
                MapImage = new BitmapImage(new System.Uri(mapPath));
            else
                MapImage = null;

            // 🧾 Обновляем текстовые поля
            LevelText = $"Уровень доступа: {SelectedMap.Level}";
            SizeText = $"Размер: {SelectedMap.Width} x {SelectedMap.Height}";
            DepthText = $"Глубина: {SelectedMap.MinDepth} - {SelectedMap.MaxDepth} м";
            DlcText = SelectedMap.DLC ? "Дополнение (DLC)" : "Базовая локация";

            OnPropertyChanged(nameof(MapImage));
            OnPropertyChanged(nameof(LevelText));
            OnPropertyChanged(nameof(SizeText));
            OnPropertyChanged(nameof(DepthText));
            OnPropertyChanged(nameof(DlcText));
            OnPropertyChanged(nameof(FishList));
        }
    }
}
