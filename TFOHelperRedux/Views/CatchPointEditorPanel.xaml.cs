using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;

namespace TFOHelperRedux.Views
{
    public partial class CatchPointEditorPanel : UserControl
    {
        private CatchPointModel? _point;

        // raised after a point is saved via SavePoint()
        public event EventHandler<CatchPointModel>? PointSaved;

        public CatchPointEditorPanel()
        {
            InitializeComponent();

            // Инициализация источников данных
            cmbMap.ItemsSource = DataStore.Maps;
            if (DataStore.Maps.Any())
                cmbMap.SelectedIndex = 0;
        }

        public void LoadPoint(CatchPointModel? point)
        {
            _point = point ?? new CatchPointModel();

            // Сброс состояний
            foreach (var f in DataStore.Fishes) f.IsSelected = _point.FishIDs?.Contains(f.ID) == true;
            foreach (var l in DataStore.Lures) l.IsSelected = _point.LureIDs?.Contains(l.ID) == true;
            foreach (var b in DataStore.Feeds) b.IsSelected = _point.FeedIDs?.Contains(b.ID) == true;
            foreach (var d in DataStore.Dips) d.IsSelected = _point.DipsIDs?.Contains(d.ID) == true;

            // Выбранная карта
            cmbMap.SelectedItem = DataStore.Maps.FirstOrDefault(m => m.ID == _point.MapID);

            // Координаты
            tbX.Text = _point.Coords?.X.ToString() ?? string.Empty;
            tbY.Text = _point.Coords?.Y.ToString() ?? string.Empty;

            // глубина
            tbDepth.Text = _point.DepthValue > 0 ? _point.DepthValue.ToString("0.0") : string.Empty;
            // клипса
            tbClip.Text = _point.ClipValue > 0 ? _point.ClipValue.ToString("0.0") : string.Empty;

            tbComment.Text = _point.Comment ?? string.Empty;

            cbTrophy.IsChecked = _point.Trophy;
            cbTournament.IsChecked = _point.Tournament;
            cbCautious.IsChecked = _point.Cautious;

            cbMorning.IsChecked = _point.Times?.Contains(1) == true;
            cbDay.IsChecked = _point.Times?.Contains(2) == true;
            cbEvening.IsChecked = _point.Times?.Contains(3) == true;
            cbNight.IsChecked = _point.Times?.Contains(4) == true;

            cbRodSpinning.IsChecked = _point.Rods?.Contains(1) == true;
            cbRodFeeder.IsChecked = _point.Rods?.Contains(2) == true;
            cbRodFloat.IsChecked = _point.Rods?.Contains(3) == true;
            cbRodFly.IsChecked = _point.Rods?.Contains(4) == true;
            cbRodSea.IsChecked = _point.Rods?.Contains(5) == true;
        }

        public CatchPointModel SavePoint()
        {
            if (cmbMap.SelectedItem is not MapModel map)
                throw new InvalidOperationException("Map not selected");

            if (!int.TryParse(tbX.Text, out var x) || !int.TryParse(tbY.Text, out var y))
                throw new InvalidOperationException("Invalid coordinates");

            // глубина
            double.TryParse(tbDepth.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var depth);
            // клипса
            double.TryParse(tbClip.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var clip);

            var fish = DataStore.Fishes.Where(f => f.IsSelected).Select(f => f.ID).ToArray();
            var lures = DataStore.Lures.Where(l => l.IsSelected).Select(l => l.ID).ToArray();
            var feeds = DataStore.Feeds.Where(f => f.IsSelected).Select(f => f.ID).ToArray();
            var dips = DataStore.Dips.Where(d => d.IsSelected).Select(d => d.ID).ToArray();

            var times = new System.Collections.Generic.List<int>();
            if (cbMorning.IsChecked == true) times.Add(1);
            if (cbDay.IsChecked == true) times.Add(2);
            if (cbEvening.IsChecked == true) times.Add(3);
            if (cbNight.IsChecked == true) times.Add(4);

            var rods = new System.Collections.Generic.List<int>();
            if (cbRodSpinning.IsChecked == true) rods.Add(1);
            if (cbRodFeeder.IsChecked == true) rods.Add(2);
            if (cbRodFloat.IsChecked == true) rods.Add(3);
            if (cbRodFly.IsChecked == true) rods.Add(4);
            if (cbRodSea.IsChecked == true) rods.Add(5);

            var point = _point ?? new CatchPointModel();

            point.MapID = map.ID;
            point.Coords = new Coords { X = x, Y = y, IsEmpty = false };
            point.DepthValue = depth;
            point.ClipValue = clip;
            point.Comment = tbComment.Text;
            point.Trophy = cbTrophy.IsChecked == true;
            point.Tournament = cbTournament.IsChecked == true;
            point.Cautious = cbCautious.IsChecked == true;
            point.FishIDs = fish;
            point.LureIDs = lures;
            point.FeedIDs = feeds;
            point.DipsIDs = dips;
            point.Times = times.ToArray();
            point.Rods = rods.ToArray();

            // Обновим вспомогательные поля для отображения
            point.MapName = DataStore.Maps.FirstOrDefault(m => m.ID == point.MapID)?.Name ?? "—";
            point.FishNames = string.Join(", ", point.FishIDs?.Select(id => DataStore.Fishes.FirstOrDefault(f => f.ID == id)?.Name) ?? new[] { "—" });

            // Вызовем событие PointSaved для подписчиков
            PointSaved?.Invoke(this, point);

            return point;
        }
    }
}
