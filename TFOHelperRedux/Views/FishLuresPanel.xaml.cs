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
            nameof(CatchPoint), typeof(CatchPointModel), typeof(FishLuresPanel), new PropertyMetadata(null));

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
            // Если установлена точка лова — сохраняем в неё
            if (CatchPoint != null)
            {
                var lureIds = CatchPoint.LureIDs?.ToList() ?? new List<int>();
                if (sender is CheckBox cb && cb.Tag is int lureId && !lureIds.Contains(lureId))
                    lureIds.Add(lureId);
                CatchPoint.LureIDs = lureIds.Distinct().ToArray();
                // Сохранение через SaveDebouncer
                DataStore.SaveAll();
            }
            // Для рыбы сохранение работает автоматически через SaveDebouncer (PropertyChange)
        }

        private void Lure_Unchecked(object sender, RoutedEventArgs e)
        {
            // Если установлена точка лова — сохраняем в неё
            if (CatchPoint != null)
            {
                var lureIds = CatchPoint.LureIDs?.ToList() ?? new List<int>();
                if (sender is CheckBox cb && cb.Tag is int lureId && lureIds.Contains(lureId))
                    lureIds.Remove(lureId);
                CatchPoint.LureIDs = lureIds.Distinct().ToArray();
                // Сохранение через SaveDebouncer
                DataStore.SaveAll();
            }
            // Для рыбы сохранение работает автоматически через SaveDebouncer (PropertyChange)
        }

        private void BestLure_Checked(object sender, RoutedEventArgs e)
        {
            // Если установлена точка лова — сохраняем в неё
            if (CatchPoint != null)
            {
                var bestLureIds = CatchPoint.BestLureIDs?.ToList() ?? new List<int>();
                if (sender is CheckBox cb && cb.Tag is int lureId && !bestLureIds.Contains(lureId))
                    bestLureIds.Add(lureId);
                CatchPoint.BestLureIDs = bestLureIds.Distinct().ToArray();
                // Сохранение через SaveDebouncer
                DataStore.SaveAll();
            }
            // Для рыбы сохранение работает автоматически через SaveDebouncer (PropertyChange)
        }

        private void BestLure_Unchecked(object sender, RoutedEventArgs e)
        {
            // Если установлена точка лова — сохраняем в неё
            if (CatchPoint != null)
            {
                var bestLureIds = CatchPoint.BestLureIDs?.ToList() ?? new List<int>();
                if (sender is CheckBox cb && cb.Tag is int lureId && bestLureIds.Contains(lureId))
                    bestLureIds.Remove(lureId);
                CatchPoint.BestLureIDs = bestLureIds.Distinct().ToArray();
                // Сохранение через SaveDebouncer
                DataStore.SaveAll();
            }
            // Для рыбы сохранение работает автоматически через SaveDebouncer (PropertyChange)
        }
    }
}
