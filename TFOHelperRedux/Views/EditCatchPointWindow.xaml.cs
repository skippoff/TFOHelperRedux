using System.Windows;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls;

namespace TFOHelperRedux.Views
{
    public partial class EditCatchPointWindow : Window
    {
        private readonly CatchPointModel _point;
        // Представления списков для фильтрации
        private readonly ListCollectionView _fishView;
        private readonly ListCollectionView _luresView;
        // Свойства для биндинга в XAML
        public ICollectionView FishView => _fishView;
        public ICollectionView LuresView => _luresView;
        
        public EditCatchPointWindow(CatchPointModel point = null)
        {
            InitializeComponent();

            _point = point ?? new CatchPointModel();
            // 🔍 создаём представления для всех коллекций DataStore
            _fishView = new ListCollectionView(DataStore.Fishes);
            _luresView = new ListCollectionView(DataStore.Lures);
            // 📌 чтобы биндинги FishView / LuresView / FeedsView / DipsView работали
            DataContext = this;
            // дальше оставляешь то, что у тебя уже было
            cmbMap.ItemsSource = DataStore.Maps;
            cmbMap.SelectedItem = DataStore.Maps.FirstOrDefault(m => m.ID == _point.MapID);
            cmbMap.ItemsSource = DataStore.Maps;

            if (DataStore.Maps.Any())
                cmbMap.SelectedIndex = 0;

            if (point != null)
                LoadPoint(point);
        }

        private void LoadPoint(CatchPointModel point)
        {
            foreach (var f in DataStore.Fishes) f.IsSelected = point.FishIDs?.Contains(f.ID) == true;
            foreach (var l in DataStore.Lures) l.IsSelected = point.LureIDs?.Contains(l.ID) == true;
            foreach (var b in DataStore.Feeds) b.IsSelected = point.FeedIDs?.Contains(b.ID) == true;
            foreach (var d in DataStore.Dips) d.IsSelected = point.DipsIDs?.Contains(d.ID) == true;

            cmbMap.SelectedItem = DataStore.Maps.FirstOrDefault(m => m.ID == point.MapID);
            tbX.Text = point.Coords.X.ToString();
            tbY.Text = point.Coords.Y.ToString();

            // глубина
            tbDepth.Text = point.DepthValue > 0
                ? point.DepthValue.ToString("0.0")
                : string.Empty;

            // клипса
            tbClip.Text = point.ClipValue > 0
                ? point.ClipValue.ToString("0.0")
                : string.Empty;

            tbComment.Text = point.Comment;

            cbTrophy.IsChecked = point.Trophy;
            cbTournament.IsChecked = point.Tournament;
            cbCautious.IsChecked = point.Cautious;

            // время
            cbMorning.IsChecked = point.Times?.Contains(1) == true;
            cbDay.IsChecked = point.Times?.Contains(2) == true;
            cbEvening.IsChecked = point.Times?.Contains(3) == true;
            cbNight.IsChecked = point.Times?.Contains(4) == true;

            // удилища
            cbRodSpinning.IsChecked = point.Rods?.Contains(1) == true;
            cbRodFeeder.IsChecked = point.Rods?.Contains(2) == true;
            cbRodFloat.IsChecked = point.Rods?.Contains(3) == true;
            cbRodFly.IsChecked = point.Rods?.Contains(4) == true;
            cbRodSea.IsChecked = point.Rods?.Contains(5) == true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMap.SelectedItem is not MapModel map)
            {
                MessageBox.Show("Выберите водоём.");
                return;
            }

            if (!int.TryParse(tbX.Text, out var x) || !int.TryParse(tbY.Text, out var y))
            {
                MessageBox.Show("Некорректные координаты.");
                return;
            }
            // глубина
            double.TryParse(
                tbDepth.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var depth);
            // клипса
            double.TryParse(
                tbClip.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var clip);

            var fish = DataStore.Fishes.Where(f => f.IsSelected).Select(f => f.ID).ToArray();
            var lures = DataStore.Lures.Where(l => l.IsSelected).Select(l => l.ID).ToArray();
            var feeds = DataStore.Feeds.Where(f => f.IsSelected).Select(f => f.ID).ToArray();
            var dips = DataStore.Dips.Where(d => d.IsSelected).Select(d => d.ID).ToArray();


            // время и удилища
            var times = new List<int>();
            if (cbMorning.IsChecked == true) times.Add(1);
            if (cbDay.IsChecked == true) times.Add(2);
            if (cbEvening.IsChecked == true) times.Add(3);
            if (cbNight.IsChecked == true) times.Add(4);

            var rods = new List<int>();
            if (cbRodSpinning.IsChecked == true) rods.Add(1);
            if (cbRodFeeder.IsChecked == true) rods.Add(2);
            if (cbRodFloat.IsChecked == true) rods.Add(3);
            if (cbRodFly.IsChecked == true) rods.Add(4);
            if (cbRodSea.IsChecked == true) rods.Add(5);

            foreach (var f in DataStore.Fishes)
                f.IsSelected = _point.FishIDs?.Contains(f.ID) == true;

            foreach (var l in DataStore.Lures)
                l.IsSelected = _point.LureIDs?.Contains(l.ID) == true;

            foreach (var b in DataStore.Feeds)
                b.IsSelected = _point.FeedIDs?.Contains(b.ID) == true;

            foreach (var d in DataStore.Dips)
                d.IsSelected = _point.DipsIDs?.Contains(d.ID) == true;


            var point = _point ?? new CatchPointModel();
            if (!_point.Equals(point))
                DataStore.CatchPoints.Add(point);
            _point.MapID = map.ID;
            _point.Coords = new Coords { X = x, Y = y, IsEmpty = false };
            _point.DepthValue = depth;
            _point.ClipValue = clip;
            _point.Comment = tbComment.Text;
            _point.Trophy = cbTrophy.IsChecked == true;
            _point.Tournament = cbTournament.IsChecked == true;
            _point.Cautious = cbCautious.IsChecked == true;
            _point.FishIDs = fish;
            _point.LureIDs = lures;
            _point.FeedIDs = feeds;
            _point.DipsIDs = dips;
            _point.Times = times.ToArray();
            _point.Rods = rods.ToArray();
            if (!TFOHelperRedux.Services.DataStore.CatchPoints.Contains(_point))
                TFOHelperRedux.Services.DataStore.CatchPoints.Add(_point);

            TFOHelperRedux.Services.DataStore.SaveAll();
            DialogResult = true;
            Close();
        }
        // Поиск по рыбам
        private void tbFishSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(_fishView, tbFishSearch.Text, item =>
            {
                if (item is FishModel f && !string.IsNullOrEmpty(f.Name))
                    return f.Name.Contains(tbFishSearch.Text, StringComparison.OrdinalIgnoreCase);

                return false;
            });
        }
        // Поиск по наживкам
        private void tbLureSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(_luresView, tbLureSearch.Text, item =>
            {
                if (item is LureModel l && !string.IsNullOrEmpty(l.Name))
                    return l.Name.Contains(tbLureSearch.Text, StringComparison.OrdinalIgnoreCase);

                return false;
            });
        }
        // Общий помощник для фильтра
        private void ApplyFilter(ListCollectionView view, string text, Predicate<object> predicate)
        {
            if (view == null)
                return;

            if (string.IsNullOrWhiteSpace(text))
            {
                // Пустая строка – показываем всех
                view.Filter = null;
            }
            else
            {
                view.Filter = o => predicate(o);
            }
        }
    }
}
