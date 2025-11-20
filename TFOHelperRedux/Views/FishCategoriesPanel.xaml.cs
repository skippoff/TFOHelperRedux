using System.Windows.Controls;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Views
{
    public partial class FishCategoriesPanel : UserControl
    {
        public FishCategoriesPanel()
        {
            InitializeComponent();
        }

        private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is FishViewModel vm &&
                sender is ListBox list &&
                list.SelectedItem is ListBoxItem item &&
                int.TryParse(item.Tag?.ToString(), out int tagId))
            {
                vm.FilterByCategory(tagId);
            }
        }
    }
}