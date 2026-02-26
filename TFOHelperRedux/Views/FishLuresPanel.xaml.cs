using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.State;

namespace TFOHelperRedux.Views
{
    public partial class FishLuresPanel : UserControl
    {
        public static readonly DependencyProperty FishViewProperty = DependencyProperty.Register(
            nameof(FishView), typeof(System.ComponentModel.ICollectionView), typeof(FishLuresPanel), new PropertyMetadata(null));

        public static readonly DependencyProperty LuresViewProperty = DependencyProperty.Register(
            nameof(LuresView), typeof(System.ComponentModel.ICollectionView), typeof(FishLuresPanel), new PropertyMetadata(null));

        public static readonly DependencyProperty CatchPointProperty = DependencyProperty.Register(
            nameof(CatchPoint), typeof(CatchPointModel), typeof(FishLuresPanel), 
            new PropertyMetadata(null, OnCatchPointChanged));

        private static void OnCatchPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Принудительно обновляем биндинги при изменении CatchPoint
            if (d is FishLuresPanel panel && e.NewValue is CatchPointModel newPoint)
            {
                // Уведомляем об изменении свойства для обновления MultiBinding
                newPoint.OnPropertyChanged(nameof(CatchPointModel.LureIDs));
                newPoint.OnPropertyChanged(nameof(CatchPointModel.BestLureIDs));
            }
        }

        public System.ComponentModel.ICollectionView? FishView
        {
            get => (System.ComponentModel.ICollectionView?)GetValue(FishViewProperty);
            set => SetValue(FishViewProperty, value);
        }

        public System.ComponentModel.ICollectionView? LuresView
        {
            get => (System.ComponentModel.ICollectionView?)GetValue(LuresViewProperty);
            set => SetValue(LuresViewProperty, value);
        }

        public CatchPointModel? CatchPoint
        {
            get => (CatchPointModel?)GetValue(CatchPointProperty);
            set => SetValue(CatchPointProperty, value);
        }

        public FishLuresPanel()
        {
            InitializeComponent();
        }

        private void TbFishSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var view = FishView;
            if (view == null)
                view = CollectionViewSource.GetDefaultView(DataContext);

            if (view != null)
            {
                var text = tbFishSearch.Text;
                if (string.IsNullOrWhiteSpace(text))
                    view.Filter = null;
                else
                    view.Filter = o => o is FishModel f && !string.IsNullOrEmpty(f.Name) && f.Name.Contains(text, System.StringComparison.OrdinalIgnoreCase);
            }
        }

        private void TbLureSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var view = LuresView;
            if (view != null)
            {
                var text = tbLureSearch.Text;
                if (string.IsNullOrWhiteSpace(text))
                    view.Filter = null;
                else
                    view.Filter = o =>
                    {
                        dynamic item = o;
                        string name = item?.Name as string;
                        return !string.IsNullOrEmpty(name) && name.Contains(text, System.StringComparison.OrdinalIgnoreCase);
                    };
            }
        }

        private void TbBestLureSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var view = LuresView;
            if (view != null)
            {
                var text = tbBestLureSearch.Text;
                if (string.IsNullOrWhiteSpace(text))
                    view.Filter = null;
                else
                    view.Filter = o =>
                    {
                        dynamic item = o;
                        string name = item?.Name as string;
                        return !string.IsNullOrEmpty(name) && name.Contains(text, System.StringComparison.OrdinalIgnoreCase);
                    };
            }
        }

        private void Lure_Checked(object sender, RoutedEventArgs e)
        {
            UpdateLureIds(sender, true, false);
        }

        private void Lure_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateLureIds(sender, false, false);
        }

        private void BestLure_Checked(object sender, RoutedEventArgs e)
        {
            UpdateLureIds(sender, true, true);
        }

        private void BestLure_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateLureIds(sender, false, true);
        }

        /// <summary>
        /// Универсальный метод обновления массивов ID наживок
        /// </summary>
        private void UpdateLureIds(object sender, bool isChecked, bool isBestLure)
        {
            if (CatchPoint == null || sender is not CheckBox cb || cb.Tag is not int lureId)
                return;

            var currentIds = isBestLure ? CatchPoint.BestLureIDs : CatchPoint.LureIDs;
            var list = currentIds?.ToList() ?? new List<int>();

            if (isChecked && !list.Contains(lureId))
                list.Add(lureId);
            else if (!isChecked && list.Contains(lureId))
                list.Remove(lureId);

            if (isBestLure)
            {
                CatchPoint.BestLureIDs = list.ToArray();
                CatchPoint.OnPropertyChanged(nameof(CatchPoint.BestLureIDs));
            }
            else
            {
                CatchPoint.LureIDs = list.ToArray();
                CatchPoint.OnPropertyChanged(nameof(CatchPoint.LureIDs));
            }
        }
    }
}
