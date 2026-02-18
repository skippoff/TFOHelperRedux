using System.Windows.Controls;
using System.Windows.Input;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.State;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Views
{
    public partial class FishDetailsPanel : UserControl
    {
        public FishDetailsPanel()
        {
            InitializeComponent();
        }
        // 🟢 Обработчик кликов по столбцам графика
        private void BiteBar_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border b && b.Tag is int hour && DataContext is FishViewModel vm)
            {
                var fish = vm.SelectedFish;
                if (fish?.BiteIntensity == null)
                    return;

                // создаём копию массива, чтобы триггерить PropertyChanged
                var arr = fish.BiteIntensity.ToArray();
                arr[hour] = (arr[hour] + 1) % 11; // увеличиваем уровень (0..10)
                fish.BiteIntensity = arr;
                vm.OnPropertyChanged(nameof(vm.SelectedFish));

                vm.RefreshSelectedFish(); // обновляем UI
                vm.OnPropertyChanged(nameof(vm.BiteDescription));
                // 💾 Автоматически сохраняем изменения
                DataService.SaveFishes(DataStore.Fishes);
            }
        }
    }
}