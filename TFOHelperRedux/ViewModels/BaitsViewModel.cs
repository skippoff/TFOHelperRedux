using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;
using TFOHelperRedux.Views;

namespace TFOHelperRedux.ViewModels
{
    /// <summary>
    /// ViewModel для управления прикормками, дипами, воблерами и компонентами
    /// </summary>
    public class BaitsViewModel : BaseViewModel
    {
        private readonly BaitCrudService _baitCrudService;

        #region Перечисление категорий

        private enum CategoryType { Feeds, FeedComponents, Dips, Lures }

        #endregion

        #region Поля

        private CategoryType _currentCategory = CategoryType.Feeds;
        private string _searchText = string.Empty;

        #endregion

        #region Коллекции данных

        public ObservableCollection<BaitModel> Feeds => DataStore.Feeds;
        public ObservableCollection<FeedComponentModel> Components => DataStore.FeedComponents;
        public ObservableCollection<DipModel> Dips => DataStore.Dips;
        public ObservableCollection<LureModel> Lures => DataStore.Lures;

        #endregion

        #region Выбранные элементы

        private BaitModel? _selectedFeed;
        public BaitModel? SelectedFeed
        {
            get => _selectedFeed;
            set { _selectedFeed = value; OnPropertyChanged(nameof(SelectedFeed)); }
        }

        private FeedComponentModel? _selectedComponent;
        public FeedComponentModel? SelectedComponent
        {
            get => _selectedComponent;
            set { _selectedComponent = value; OnPropertyChanged(nameof(SelectedComponent)); }
        }

        private DipModel? _selectedDip;
        public DipModel? SelectedDip
        {
            get => _selectedDip;
            set { _selectedDip = value; OnPropertyChanged(nameof(SelectedDip)); }
        }

        private LureModel? _selectedLure;
        public LureModel? SelectedLure
        {
            get => _selectedLure;
            set { _selectedLure = value; OnPropertyChanged(nameof(SelectedLure)); }
        }

        #endregion

        #region Поиск и фильтрация

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

        private ObservableCollection<object> _filteredItems = new();
        public ObservableCollection<object> FilteredItems
        {
            get => _filteredItems;
            set { _filteredItems = value; OnPropertyChanged(nameof(FilteredItems)); }
        }

        #endregion

        #region Команды навигации по категориям

        public ICommand ShowFeedsCmd { get; }
        public ICommand ShowComponentsCmd { get; }
        public ICommand ShowDipsCmd { get; }
        public ICommand ShowLuresCmd { get; }

        #endregion

        #region Команды редактирования

        public ICommand AddItemCmd { get; }
        public ICommand EditItemCmd { get; }
        public ICommand DeleteItemCmd { get; }

        #endregion

        #region Конструктор

        public BaitsViewModel()
        {
            _baitCrudService = new BaitCrudService(new FishDataService());

            // Инициализация команд навигации
            ShowFeedsCmd = new RelayCommand(() => SwitchCategory(CategoryType.Feeds));
            ShowComponentsCmd = new RelayCommand(() => SwitchCategory(CategoryType.FeedComponents));
            ShowDipsCmd = new RelayCommand(() => SwitchCategory(CategoryType.Dips));
            ShowLuresCmd = new RelayCommand(() => SwitchCategory(CategoryType.Lures));

            // Инициализация команд редактирования
            AddItemCmd = new RelayCommand(AddItem);
            EditItemCmd = new RelayCommand(EditItem, CanEditItem);
            DeleteItemCmd = new RelayCommand(DeleteItem, CanDeleteItem);

            // Загрузка начальной категории
            LoadCurrentCategory();
        }

        #endregion

        #region Методы навигации по категориям

        private void SwitchCategory(CategoryType category)
        {
            _currentCategory = category;
            LoadCurrentCategory();
            ApplyFilter();
        }

        private void LoadCurrentCategory()
        {
            FilteredItems = _currentCategory switch
            {
                CategoryType.Feeds => new ObservableCollection<object>(DataStore.Feeds.Cast<object>()),
                CategoryType.FeedComponents => new ObservableCollection<object>(DataStore.FeedComponents.Cast<object>()),
                CategoryType.Dips => new ObservableCollection<object>(DataStore.Dips.Cast<object>()),
                CategoryType.Lures => new ObservableCollection<object>(DataStore.Lures.Cast<object>()),
                _ => new ObservableCollection<object>()
            };

            OnPropertyChanged(nameof(FilteredItems));
        }

        private void ApplyFilter()
        {
            var filtered = _currentCategory switch
            {
                CategoryType.Feeds => new ObservableCollection<object>(
                    DataStore.Feeds
                        .Where(i => i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                        .Cast<object>()),

                CategoryType.FeedComponents => new ObservableCollection<object>(
                    DataStore.FeedComponents
                        .Where(i => i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                        .Cast<object>()),

                CategoryType.Dips => new ObservableCollection<object>(
                    DataStore.Dips
                        .Where(i => i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                        .Cast<object>()),

                CategoryType.Lures => new ObservableCollection<object>(
                    DataStore.Lures
                        .Where(i => i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                        .Cast<object>()),

                _ => new ObservableCollection<object>()
            };

            FilteredItems = filtered;
        }

        #endregion

        #region Методы редактирования

        private bool CanEditItem() => true;
        private bool CanDeleteItem() => GetSelectedItem() != null;

        private IItemModel? GetSelectedItem() => _currentCategory switch
        {
            CategoryType.Feeds => SelectedFeed,
            CategoryType.FeedComponents => SelectedComponent,
            CategoryType.Dips => SelectedDip,
            CategoryType.Lures => SelectedLure,
            _ => null
        };

        private void AddItem()
        {
            IItemModel newItem = _currentCategory switch
            {
                CategoryType.Feeds => _baitCrudService.CreateFeed(),
                CategoryType.FeedComponents => _baitCrudService.CreateComponent(),
                CategoryType.Dips => _baitCrudService.CreateDip(),
                CategoryType.Lures => _baitCrudService.CreateLure(),
                _ => throw new InvalidOperationException("Неизвестная категория")
            };

            var window = new EditItemWindow(newItem) { Owner = Application.Current.MainWindow };
            if (window.ShowDialog() == true)
            {
                _baitCrudService.AddToCollection(newItem);
                _baitCrudService.SaveItem(newItem);
                LoadCurrentCategory();
            }
        }

        private void EditItem()
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите элемент для редактирования.", "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var window = new EditItemWindow(selectedItem) { Owner = Application.Current.MainWindow };
            if (window.ShowDialog() == true)
            {
                _baitCrudService.SaveItem(selectedItem);
                LoadCurrentCategory();
            }
        }

        private void DeleteItem()
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите элемент для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("Удалить выбранный элемент?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            _baitCrudService.RemoveFromCollection(selectedItem);
            _baitCrudService.SaveItem(selectedItem);
            SelectedFeed = null;
            SelectedComponent = null;
            SelectedDip = null;
            SelectedLure = null;
            LoadCurrentCategory();
        }

        #endregion

        #region Публичные методы для обновления UI

        public void RefreshCollections()
        {
            OnPropertyChanged(nameof(Feeds));
            OnPropertyChanged(nameof(Components));
            OnPropertyChanged(nameof(Dips));
            OnPropertyChanged(nameof(Lures));
            LoadCurrentCategory();
        }

        #endregion
    }
}
