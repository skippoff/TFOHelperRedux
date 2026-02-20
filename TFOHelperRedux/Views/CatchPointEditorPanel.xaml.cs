using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Business;
using TFOHelperRedux.Services.Data;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;

namespace TFOHelperRedux.Views
{
    public partial class CatchPointEditorPanel : UserControl, System.ComponentModel.INotifyPropertyChanged
    {
        private CatchPointModel? _point;

        // raised after a point is saved via SavePoint()
        public event EventHandler<CatchPointModel>? PointSaved;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));

        private string _selectedLargeText = string.Empty;
        public string SelectedLargeText
        {
            get => _selectedLargeText;
            set
            {
                if (_selectedLargeText == value) return;
                _selectedLargeText = value;
                OnPropertyChanged(nameof(SelectedLargeText));
                ApplyLargeToSelectedFish();
            }
        }

        private string _selectedTrophyText = string.Empty;
        public string SelectedTrophyText
        {
            get => _selectedTrophyText;
            set
            {
                if (_selectedTrophyText == value) return;
                _selectedTrophyText = value;
                OnPropertyChanged(nameof(SelectedTrophyText));
                ApplyTrophyToSelectedFish();
            }
        }

        public CatchPointEditorPanel()
        {
            InitializeComponent();

            // Инициализация источников данных
            cmbMap.ItemsSource = DataStore.Maps;
            if (DataStore.Maps.Any())
                cmbMap.SelectedIndex = 0;

            // Подписываемся на изменения в коллекции рыб, чтобы обновлять поля веса
            DataStore.Fishes.CollectionChanged += Fishes_CollectionChanged;
            foreach (var f in DataStore.Fishes)
                AttachFishHandler(f);
        }

        private void Fishes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                    if (item is FishModel fm)
                        AttachFishHandler(fm);
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                    if (item is FishModel fm)
                        DetachFishHandler(fm);
            }

            UpdateWeightFieldsFromSelection();
        }

        private void AttachFishHandler(FishModel fish)
        {
            if (fish is INotifyPropertyChanged npc)
                npc.PropertyChanged += Fish_PropertyChanged;
        }

        private void DetachFishHandler(FishModel fish)
        {
            if (fish is INotifyPropertyChanged npc)
                npc.PropertyChanged -= Fish_PropertyChanged;
        }

        private void Fish_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FishModel.IsSelected))
            {
                // обновляем поля веса при смене выбора рыбы
                UpdateWeightFieldsFromSelection();
            }
        }

        private void UpdateWeightFieldsFromSelection()
        {
            var selected = DataStore.Fishes.Where(f => f.IsSelected).ToList();
            if (selected.Count == 1)
            {
                SelectedLargeText = selected[0].WeightLarge.ToString();
                SelectedTrophyText = selected[0].WeightTrophy.ToString();
            }
            else
            {
                SelectedLargeText = string.Empty;
                SelectedTrophyText = string.Empty;
            }
        }

        private void WeightField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveWeightsToSelectedFish();
                e.Handled = true;
                Keyboard.ClearFocus();
            }
        }

        private void WeightField_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveWeightsToSelectedFish();
        }

        private void SaveWeightsToSelectedFish()
        {
            var selected = DataStore.Fishes.Where(f => f.IsSelected).ToList();
            if (selected.Count != 1)
                return;

            var fish = selected[0];

            if (int.TryParse(SelectedLargeText, out var large))
                fish.WeightLarge = large;

            if (int.TryParse(SelectedTrophyText, out var trophy))
                fish.WeightTrophy = trophy;

            // Сохраняем изменения
            DataService.SaveFishes(DataStore.Fishes);

            // Обновляем представления
            if (App.Current?.MainWindow?.DataContext is TFOHelperRedux.ViewModels.FishViewModel vm)
                vm.RefreshSelectedFish();
        }

        private void ApplyLargeToSelectedFish()
        {
            // При изменении текста большого веса применяем его к выбранной рыбе (если выбрана одна)
            var selected = DataStore.Fishes.Where(f => f.IsSelected).ToList();
            if (selected.Count != 1) return;

            if (int.TryParse(SelectedLargeText, out var large))
            {
                selected[0].WeightLarge = large;
                DataService.SaveFishes(DataStore.Fishes);
                if (App.Current?.MainWindow?.DataContext is TFOHelperRedux.ViewModels.FishViewModel vm)
                    vm.RefreshSelectedFish();
            }
        }

        private void ApplyTrophyToSelectedFish()
        {
            // При изменении текста трофейного веса применяем его к выбранной рыбе (если выбрана одна)
            var selected = DataStore.Fishes.Where(f => f.IsSelected).ToList();
            if (selected.Count != 1) return;

            if (int.TryParse(SelectedTrophyText, out var trophy))
            {
                selected[0].WeightTrophy = trophy;
                DataService.SaveFishes(DataStore.Fishes);
                if (App.Current?.MainWindow?.DataContext is TFOHelperRedux.ViewModels.FishViewModel vm)
                    vm.RefreshSelectedFish();
            }
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

            // Обновляем поля веса в зависимости от выбранных рыб
            UpdateWeightFieldsFromSelection();

            // Если в точке не выбрана ни одна рыба, заполнить поля значениями из глобально выбранной рыбы (DataStore.Selection.SelectedFish)
            var anySelected = DataStore.Fishes.Any(f => f.IsSelected);
            if (!anySelected && DataStore.Selection.SelectedFish != null)
            {
                SelectedLargeText = DataStore.Selection.SelectedFish.WeightLarge.ToString();
                SelectedTrophyText = DataStore.Selection.SelectedFish.WeightTrophy.ToString();
            }
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
