using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;
using TFOHelperRedux.Views;

namespace TFOHelperRedux.ViewModels
{
    public class BaitsDipsViewModel : BaseViewModel
    {
        private enum Category { Lures, Feeds, Dips }

        private Category _currentCategory = Category.Lures;
        private string _searchText = "";
        public ObservableCollection<object> FilteredItems { get; private set; } = new();
        public ICommand ShowLuresCmd { get; }
        public ICommand ShowFeedsCmd { get; }
        public ICommand ShowDipsCmd { get; }
        public ICommand AddItemCmd { get; }
        public ICommand EditItemCmd { get; }
        public ICommand DeleteItemCmd { get; }

        private object? _selectedItem;
        public object? SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(nameof(SelectedItem)); }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    ApplyFilter();
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        public BaitsDipsViewModel()
        {
            ShowLuresCmd = new RelayCommand(() => SwitchCategory(Category.Lures));
            ShowFeedsCmd = new RelayCommand(() => SwitchCategory(Category.Feeds));
            ShowDipsCmd = new RelayCommand(() => SwitchCategory(Category.Dips));

            AddItemCmd = new RelayCommand(() => AddItem());
            EditItemCmd = new RelayCommand(() => EditItem());
            DeleteItemCmd = new RelayCommand(() => DeleteItem());

            LoadCurrentCategory();
        }

        private void SwitchCategory(Category cat)
        {
            _currentCategory = cat;
            ApplyFilter();
        }

        private void LoadCurrentCategory()
        {
            switch (_currentCategory)
            {
                case Category.Lures:
                    FilteredItems = new ObservableCollection<object>(DataStore.Lures.Cast<object>());
                    break;
                case Category.Feeds:
                    FilteredItems = new ObservableCollection<object>(DataStore.Feeds.Cast<object>());
                    break;
                case Category.Dips:
                    FilteredItems = new ObservableCollection<object>(DataStore.Dips.Cast<object>());
                    break;
            }
            OnPropertyChanged(nameof(FilteredItems));
        }

        private void ApplyFilter()
        {
            ObservableCollection<object> source = _currentCategory switch
            {
                Category.Lures => new(DataStore.Lures.Where(i =>
                    i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).Cast<object>()),
                Category.Feeds => new(DataStore.Feeds.Where(i =>
                    i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).Cast<object>()),
                Category.Dips => new(DataStore.Dips.Where(i =>
                    i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).Cast<object>()),
                _ => new()
            };

            FilteredItems = source;
            OnPropertyChanged(nameof(FilteredItems));
        }

        private void AddItem()
        {
            IItemModel newItem = _currentCategory switch
            {
                Category.Lures => new LureModel(),
                Category.Feeds => new BaitModel(),
                Category.Dips => new DipModel(),
                _ => throw new InvalidOperationException()
            };

            // вычисляем новый ID
            switch (_currentCategory)
            {
                case Category.Lures:
                    newItem.ID = (DataStore.Lures.Any() ? DataStore.Lures.Max(x => x.ID) : 0) + 1;
                    break;
                case Category.Feeds:
                    newItem.ID = (DataStore.Feeds.Any() ? DataStore.Feeds.Max(x => x.ID) : 0) + 1;
                    break;
                case Category.Dips:
                    newItem.ID = (DataStore.Dips.Any() ? DataStore.Dips.Max(x => x.ID) : 0) + 1;
                    break;
            }

            var win = new EditItemWindow(newItem) { Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true)
            {
                switch (_currentCategory)
                {
                    case Category.Lures:
                        DataStore.Lures.Add((LureModel)newItem);
                        JsonService.Save(DataService.LuresJson, DataStore.Lures);
                        break;
                    case Category.Feeds:
                        DataStore.Feeds.Add((BaitModel)newItem);
                        JsonService.Save(DataService.FeedsJson, DataStore.Feeds);
                        break;
                    case Category.Dips:
                        DataStore.Dips.Add((DipModel)newItem);
                        JsonService.Save(DataService.DipsJson, DataStore.Dips);
                        break;
                }
                ApplyFilter();
            }
        }

        private void EditItem()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Выберите элемент для редактирования.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedItem is IItemModel item)
            {
                // Передаём оригинал в окно (оно копирует и применяет изменения только при OK)
                var win = new EditItemWindow(item) { Owner = Application.Current.MainWindow };
                if (win.ShowDialog() == true)
                {
                    // Сохраняем соответствующий JSON
                    switch (_currentCategory)
                    {
                        case Category.Lures:
                            JsonService.Save(DataService.LuresJson, DataStore.Lures);
                            break;
                        case Category.Feeds:
                            JsonService.Save(DataService.FeedsJson, DataStore.Feeds);
                            break;
                        case Category.Dips:
                            JsonService.Save(DataService.DipsJson, DataStore.Dips);
                            break;
                    }
                    ApplyFilter();
                }
            }
        }

        private void DeleteItem()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Выберите элемент для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("Удалить выбранный элемент?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            switch (_currentCategory)
            {
                case Category.Lures:
                    if (SelectedItem is LureModel lm) DataStore.Lures.Remove(lm);
                    JsonService.Save(DataService.LuresJson, DataStore.Lures);
                    break;
                case Category.Feeds:
                    if (SelectedItem is BaitModel bm) DataStore.Feeds.Remove(bm);
                    JsonService.Save(DataService.FeedsJson, DataStore.Feeds);
                    break;
                case Category.Dips:
                    if (SelectedItem is DipModel dm) DataStore.Dips.Remove(dm);
                    JsonService.Save(DataService.DipsJson, DataStore.Dips);
                    break;
            }

            SelectedItem = null;
            ApplyFilter();
        }
    }
}
