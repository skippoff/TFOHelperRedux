using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.State;
using TFOHelperRedux.ViewModels;
using TFOHelperRedux.Views.Controls;

namespace TFOHelperRedux.Views
{
    public partial class FishDetailsPanel : UserControl
    {
        public FishDetailsPanel()
        {
            InitializeComponent();
            Loaded += FishDetailsPanel_Loaded;
        }

        private void FishDetailsPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is FishViewModel vm)
            {
                vm.PropertyChanged += ViewModel_PropertyChanged;
                UpdateVisibility(vm.SelectedFish);
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FishViewModel.SelectedFish))
            {
                UpdateVisibility((sender as FishViewModel)?.SelectedFish);
            }
        }

        private void UpdateVisibility(object? selectedFish)
        {
            var isVisible = selectedFish != null;
            FishDetailsContent.Visibility = isVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            FishPlaceholder.Visibility = isVisible ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        // 🟢 Обработчик изменения интенсивности клёва из контрола графика
        private void BiteChart_HourChanged(object? sender, HourChangedEventArgs e)
        {
            if (DataContext is FishViewModel vm)
            {
                var fish = vm.SelectedFish;
                if (fish == null)
                    return;

                vm.RefreshSelectedFish(); // обновляем UI
                vm.OnPropertyChanged(nameof(vm.BiteDescription));
                // 💾 Автоматически сохраняем изменения
                DataService.SaveFishes(DataStore.Fishes);
            }
        }
    }
}