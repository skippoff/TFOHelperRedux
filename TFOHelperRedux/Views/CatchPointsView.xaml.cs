using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Business;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.State;

namespace TFOHelperRedux.Views;

public partial class CatchPointsView : UserControl
{
    public CatchPointsView()
    {
        InitializeComponent();
        // DataContext устанавливается из родителя (FishDetailsPanel → FishViewModel → CatchPointsVM)
    }

    private void CatchPoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border border || border.DataContext is not CatchPointModel point)
            return;

        // Если клик был по кнопке внутри карточки – ничего не делаем,
        // кнопка сама обработает команду
        if (e.OriginalSource is DependencyObject dep)
        {
            var current = dep;
            while (current != null && current != border)
            {
                if (current is Button)
                    return;

                current = VisualTreeHelper.GetParent(current);
            }
        }

        // 🟢 Один клик → пока без действия (резерв для будущего функционала)
        if (e.ClickCount == 1)
        {
            // Клик по карточке больше не открывает карту
            // Для открытия карты используйте кнопку 🗺 в левом верхнем углу карточки
        }
        // 🔵 Двойной клик → редактирование точки
        else if (e.ClickCount == 2)
        {
            var wnd = new TFOHelperRedux.Views.EditCatchPointWindow(point);
            if (wnd.ShowDialog() == true &&
                DataContext is TFOHelperRedux.ViewModels.CatchPointsViewModel vm)
            {
                var fish = DataStore.Selection.SelectedFish ?? vm.CurrentFish;
                vm.RefreshFilteredPoints(fish);
            }
        }

        e.Handled = true;
    }
}
