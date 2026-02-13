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

            // Инициализация левой панели (она сама заполнит cmbMap)
            LeftPanel.PointSaved += (s, savedPoint) =>
            {
                // Обновляем DataStore и VM после сохранения
                if (!TFOHelperRedux.Services.DataStore.CatchPoints.Contains(savedPoint))
                    TFOHelperRedux.Services.DataStore.CatchPoints.Add(savedPoint);

                // обновим вспомогательные коллекции
                var vm = App.Current.MainWindow?.DataContext as TFOHelperRedux.ViewModels.FishViewModel;
                if (vm != null)
                    vm.CatchPointsVM.RefreshFilteredPoints(TFOHelperRedux.Services.DataStore.SelectedFish);

                TFOHelperRedux.Services.DataStore.SaveAll();
            };

            if (point != null)
                LeftPanel.LoadPoint(point);
            else
                LeftPanel.LoadPoint(null);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var point = LeftPanel.SavePoint();

                if (!TFOHelperRedux.Services.DataStore.CatchPoints.Contains(point))
                    TFOHelperRedux.Services.DataStore.CatchPoints.Add(point);

                TFOHelperRedux.Services.DataStore.SaveAll();
                DialogResult = true;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        // Поиск по рыбам
        private void tbFishSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(_fishView, tbFishSearch.Text, item =>
            {
                if (item is FishModel f && !string.IsNullOrEmpty(f.Name))
                    return f.Name.Contains(tbFishSearch.Text, System.StringComparison.OrdinalIgnoreCase);

                return false;
            });
        }
        // Поиск по наживкам
        private void tbLureSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(_luresView, tbLureSearch.Text, item =>
            {
                if (item is LureModel l && !string.IsNullOrEmpty(l.Name))
                    return l.Name.Contains(tbLureSearch.Text, System.StringComparison.OrdinalIgnoreCase);

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
