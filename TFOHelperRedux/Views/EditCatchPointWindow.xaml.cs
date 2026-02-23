using System;
using System.Linq;
using System.Windows;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Business;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.State;
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
            // 📌 чтобы биндинги FishView / LuresView работали
            DataContext = this;
            
            // Устанавливаем выбранную точку лова
            DataStore.Selection.SelectedCatchPoint = _point;

            // Устанавливаем DataContext и CatchPoint для FishFeedsPanel
            var mainVM = App.Current.MainWindow?.DataContext as TFOHelperRedux.ViewModels.FishViewModel;
            if (mainVM != null)
            {
                RightPanel.DataContext = mainVM.FishFeedsVM;
                RightPanel.CatchPoint = _point;
            }
            else
            {
                // Если главное окно ещё не инициализировано, используем ServiceContainer
                var fishFeedsVM = TFOHelperRedux.Services.DI.ServiceContainer.GetService<TFOHelperRedux.ViewModels.FishFeedsViewModel>();
                if (fishFeedsVM != null)
                {
                    RightPanel.DataContext = fishFeedsVM;
                    RightPanel.CatchPoint = _point;
                }
            }
            
            // Восстанавливаем чекбоксы прикормок из первой выбранной рыбы
            var selectedFish = DataStore.Fishes.FirstOrDefault(f => f.IsSelected);
            if (selectedFish != null && RightPanel.DataContext is TFOHelperRedux.ViewModels.FishFeedsViewModel feedsVM)
            {
                // Синхронизируем чекбоксы с данными рыбы
                feedsVM.SyncWithFish(selectedFish);
            }

            // Синхронизируем чекбоксы наживок с данными рыбы
            if (selectedFish != null)
                SyncLuresWithFish(selectedFish);

            // Инициализация левой панели (она сама заполнит cmbMap)
            LeftPanel.PointSaved += (s, savedPoint) =>
            {
                // Обновляем DataStore и VM после сохранения
                // Не добавляем точку, если она уже есть в коллекции (по ссылке)
                if (!ReferenceEquals(savedPoint, _point) && !DataStore.CatchPoints.Contains(savedPoint))
                    DataStore.CatchPoints.Add(savedPoint);

                // обновим вспомогательные коллекции
                var vm = App.Current.MainWindow?.DataContext as TFOHelperRedux.ViewModels.FishViewModel;
                if (vm != null)
                    vm.CatchPointsVM.RefreshFilteredPoints(DataStore.Selection.SelectedFish);

                DataStore.SaveAll();
            };

            if (point != null)
                LeftPanel.LoadPoint(point);
            else
                LeftPanel.LoadPoint(null);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Очищаем выбранную точку при закрытии
            DataStore.Selection.SelectedCatchPoint = null;
            Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Сначала получаем RecipeIDs и FeedIDs из FishFeedsPanel
                var recipeIds = RightPanel.SelectedRecipeIds;
                var feedIds = RightPanel.SelectedFeedIds;
                
                var point = LeftPanel.SavePoint();
                
                // Сохраняем прикормки в первую выбранную рыбу
                var selectedFish = DataStore.Fishes.FirstOrDefault(f => f.IsSelected);
                if (selectedFish != null)
                {
                    selectedFish.FeedIDs = feedIds;
                    selectedFish.RecipeIDs = recipeIds;
                    DataService.SaveFishes(DataStore.Fishes);
                }
                
                // Для точки лова сохраняем только те прикормки, которые выбраны для рыбы
                point.FeedIDs = feedIds;
                point.RecipeIDs = recipeIds;

                // Находим точку в коллекции и обновляем её (если это редактирование)
                var existingPoint = DataStore.CatchPoints.FirstOrDefault(p => ReferenceEquals(p, _point) || 
                    (p.MapID == point.MapID && p.Coords.X == point.Coords.X && p.Coords.Y == point.Coords.Y));
                
                if (existingPoint != null && existingPoint != point)
                {
                    // Обновляем существующую точку
                    existingPoint.RecipeIDs = point.RecipeIDs;
                    existingPoint.FeedIDs = point.FeedIDs;
                    existingPoint.LureIDs = point.LureIDs;
                    existingPoint.FishIDs = point.FishIDs;
                    existingPoint.DipsIDs = point.DipsIDs;
                    existingPoint.Comment = point.Comment;
                    existingPoint.Times = point.Times;
                    existingPoint.Rods = point.Rods;
                    existingPoint.Trophy = point.Trophy;
                    existingPoint.Tournament = point.Tournament;
                    existingPoint.Cautious = point.Cautious;
                    existingPoint.DepthValue = point.DepthValue;
                    existingPoint.ClipValue = point.ClipValue;
                    existingPoint.DateEdited = DateTime.Now;
                }
                else
                {
                    // Добавляем новую точку
                    if (!DataStore.CatchPoints.Contains(point))
                        DataStore.CatchPoints.Add(point);
                }

                DataStore.SaveAll();
                DialogResult = true;
                
                // Очищаем выбранную точку после успешного сохранения
                DataStore.Selection.SelectedCatchPoint = null;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Синхронизирует чекбоксы наживок с LureIDs и BestLureIDs выбранной рыбы
        /// </summary>
        private void SyncLuresWithFish(FishModel fish)
        {
            if (DataStore.Lures == null)
                return;

            var lureIdsSet = fish.LureIDs != null && fish.LureIDs.Length > 0
                ? new HashSet<int>(fish.LureIDs)
                : null;

            var bestLureIdsSet = fish.BestLureIDs != null && fish.BestLureIDs.Length > 0
                ? new HashSet<int>(fish.BestLureIDs)
                : null;

            foreach (var lure in DataStore.Lures)
            {
                var shouldBeSelected = lureIdsSet?.Contains(lure.ID) ?? false;
                var shouldBeBestSelected = bestLureIdsSet?.Contains(lure.ID) ?? false;

                if (lure.IsSelected != shouldBeSelected)
                    lure.IsSelected = shouldBeSelected;

                if (lure.IsBestSelected != shouldBeBestSelected)
                    lure.IsBestSelected = shouldBeBestSelected;
            }
        }
    }
}
