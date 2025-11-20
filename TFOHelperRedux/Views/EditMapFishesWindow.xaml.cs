using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;

namespace TFOHelperRedux.Views
{
    public partial class EditMapFishesWindow : Window
    {
        private readonly MapModel _map;

        public ObservableCollection<MapFishItem> Items { get; } = new();

        public EditMapFishesWindow(MapModel map)
        {
            InitializeComponent();
            _map = map;

            txtHeader.Text = $"Рыбы на локации: {map.Name}";

            // Строим список всех рыб
            foreach (var fish in DataStore.Fishes.OrderBy(f => f.Name))
            {
                Items.Add(new MapFishItem(fish, _map));
            }

            FishItemsControl.ItemsSource = Items;
        }
        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(FishItemsControl.ItemsSource);
            if (view == null)
                return;

            var text = tbSearch.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                // Пустой поиск – показываем всех
                view.Filter = null;
            }
            else
            {
                text = text.Trim();

                view.Filter = item =>
                {
                    if (item is MapFishItem mfi && !string.IsNullOrEmpty(mfi.Fish.Name))
                        return mfi.Fish.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;

                    return false;
                };
            }

            view.Refresh();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class MapFishItem : INotifyPropertyChanged
    {
        private readonly MapModel _map;
        public FishModel Fish { get; }

        private bool _isOnMap;
        public bool IsOnMap
        {
            get => _isOnMap;
            set
            {
                if (_isOnMap != value)
                {
                    _isOnMap = value;
                    OnPropertyChanged(nameof(IsOnMap));
                    UpdateMap();
                }
            }
        }

        public MapFishItem(FishModel fish, MapModel map)
        {
            Fish = fish;
            _map = map;

            var ids = _map.FishIDs ?? Array.Empty<int>();
            _isOnMap = ids.Contains(Fish.ID);
        }

        private void UpdateMap()
        {
            var ids = _map.FishIDs ?? Array.Empty<int>();

            if (_isOnMap)
            {
                if (!ids.Contains(Fish.ID))
                    ids = ids.Concat(new[] { Fish.ID })
                             .Distinct()
                             .OrderBy(x => x)
                             .ToArray();
            }
            else
            {
                if (ids.Contains(Fish.ID))
                    ids = ids.Where(id => id != Fish.ID).ToArray();
            }

            _map.FishIDs = ids;

            // 💾 сохраняем все карты
            DataService.SaveMaps(DataStore.Maps);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
