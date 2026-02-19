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
            if (sender is CheckBox cb && cb.Tag is int id)
            {
                var fish = DataStore.Selection.SelectedFish;
                if (fish == null) return;

                fish.LureIDs = (fish.LureIDs ?? Array.Empty<int>()).Concat(new[] { id }).Distinct().ToArray();
                DataService.SaveFishes(DataStore.Fishes);

                // Обновляем UI рекомендованных наживок
                if (this.DataContext is ViewModels.FishViewModel fishVm)
                    fishVm.OnPropertyChanged(nameof(ViewModels.FishViewModel.MaybeCatchLures));
            }
        }

        private void Lure_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is int id)
            {
                var fish = DataStore.Selection.SelectedFish;
                if (fish == null) return;

                fish.LureIDs = (fish.LureIDs ?? Array.Empty<int>()).Where(x => x != id).ToArray();
                DataService.SaveFishes(DataStore.Fishes);

                // Обновляем UI рекомендованных наживок
                if (this.DataContext is ViewModels.FishViewModel fishVm)
                    fishVm.OnPropertyChanged(nameof(ViewModels.FishViewModel.MaybeCatchLures));
            }
        }

        private void BestLure_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is int id)
            {
                var fish = DataStore.Selection.SelectedFish;
                if (fish == null) return;

                fish.BestLureIDs = (fish.BestLureIDs ?? Array.Empty<int>()).Concat(new[] { id }).Distinct().ToArray();
                DataService.SaveFishes(DataStore.Fishes);

                // Обновляем UI лучших наживок
                if (this.DataContext is ViewModels.FishViewModel fishVm)
                    fishVm.OnPropertyChanged(nameof(ViewModels.FishViewModel.BestLures));
            }
        }

        private void BestLure_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is int id)
            {
                var fish = DataStore.Selection.SelectedFish;
                if (fish == null) return;

                fish.BestLureIDs = (fish.BestLureIDs ?? Array.Empty<int>()).Where(x => x != id).ToArray();
                DataService.SaveFishes(DataStore.Fishes);

                // Обновляем UI лучших наживок
                if (this.DataContext is ViewModels.FishViewModel fishVm)
                    fishVm.OnPropertyChanged(nameof(ViewModels.FishViewModel.BestLures));
            }
        }
    }
}
