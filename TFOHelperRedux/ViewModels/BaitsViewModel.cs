using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Business;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.UI;

namespace TFOHelperRedux.ViewModels
{
    /// <summary>
    /// ViewModel для управления прикормками, дипами, воблерами и компонентами
    /// </summary>
    public class BaitsViewModel : BaseViewModel
    {
        private readonly BaitCrudService _baitCrudService;
        private readonly IUIService _uiService;

        #region Перечисление категорий

        private enum CategoryType { Feeds, FeedComponents, Dips, Lures }

        #endregion

        #region Поля

        private CategoryType _currentCategory = CategoryType.Feeds;
        private string _searchText = string.Empty;
        private IItemModel? _selectedItem;

        #endregion

        #region Коллекции данных (публичные для XAML)

        public ObservableCollection<BaitModel> Feeds => DataStore.Feeds;
        public ObservableCollection<FeedComponentModel> Components => DataStore.FeedComponents;
        public ObservableCollection<DipModel> Dips => DataStore.Dips;
        public ObservableCollection<LureModel> Lures => DataStore.Lures;

        /// <summary>
        /// Текущая коллекция элементов в зависимости от категории (типобезопасно)
        /// </summary>
        public ObservableCollection<IItemModel> CurrentItems { get; private set; } = new();

        #endregion

        #region Выбранный элемент (единый для всех категорий)

        public IItemModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                    OnPropertyChanged(nameof(CanDeleteItem));
                }
            }
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

        #endregion

        #region Команды навигации по категориям

        public ICommand ShowFeedsCmd { get; }
        public ICommand ShowComponentsCmd { get; }
        public ICommand ShowDipsCmd { get; }
        public ICommand ShowLuresCmd { get; }

        #endregion

        #region Команды редактирования

        public ICommand DeleteItemCmd { get; }

        #endregion

        #region Конструктор

        public BaitsViewModel(BaitCrudService baitCrudService, IUIService uiService)
        {
            _baitCrudService = baitCrudService;
            _uiService = uiService;

            // Инициализация команд навигации
            ShowFeedsCmd = new RelayCommand(() => SwitchCategory(CategoryType.Feeds));
            ShowComponentsCmd = new RelayCommand(() => SwitchCategory(CategoryType.FeedComponents));
            ShowDipsCmd = new RelayCommand(() => SwitchCategory(CategoryType.Dips));
            ShowLuresCmd = new RelayCommand(() => SwitchCategory(CategoryType.Lures));

            // Инициализация команд редактирования
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

        public void LoadCurrentCategory()
        {
            CurrentItems.Clear();
            
            var items = _currentCategory switch
            {
                CategoryType.Feeds => DataStore.Feeds.Cast<IItemModel>(),
                CategoryType.FeedComponents => DataStore.FeedComponents.Cast<IItemModel>(),
                CategoryType.Dips => DataStore.Dips.Cast<IItemModel>(),
                CategoryType.Lures => DataStore.Lures.Cast<IItemModel>(),
                _ => Enumerable.Empty<IItemModel>()
            };

            foreach (var item in items)
                CurrentItems.Add(item);

            OnPropertyChanged(nameof(CurrentItems));
            OnPropertyChanged(nameof(SelectedItem));
            SelectedItem = null;
        }

        /// <summary>
        /// Переключает категорию по имени (для интеграции с NavigationVM)
        /// </summary>
        public void SetCategory(string categoryName)
        {
            _currentCategory = categoryName switch
            {
                "Feeds" => CategoryType.Feeds,
                "FeedComponents" => CategoryType.FeedComponents,
                "Dips" => CategoryType.Dips,
                "Lures" => CategoryType.Lures,
                _ => _currentCategory
            };
            
            LoadCurrentCategory();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            CurrentItems.Clear();
            
            var items = _currentCategory switch
            {
                CategoryType.Feeds => DataStore.Feeds
                    .Where(i => i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .Cast<IItemModel>(),

                CategoryType.FeedComponents => DataStore.FeedComponents
                    .Where(i => i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .Cast<IItemModel>(),

                CategoryType.Dips => DataStore.Dips
                    .Where(i => i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .Cast<IItemModel>(),

                CategoryType.Lures => DataStore.Lures
                    .Where(i => i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .Cast<IItemModel>(),

                _ => Enumerable.Empty<IItemModel>()
            };

            foreach (var item in items)
                CurrentItems.Add(item);

            OnPropertyChanged(nameof(CurrentItems));
        }

        #endregion

        #region Методы редактирования

        private bool CanDeleteItem() => SelectedItem != null;

        private void DeleteItem()
        {
            if (SelectedItem == null)
            {
                _uiService.ShowInfo("Выберите элемент для удаления.", "Удаление");
                return;
            }

            if (!_uiService.ShowConfirm("Удалить выбранный элемент?", "Подтверждение"))
                return;

            _baitCrudService.RemoveFromCollection(SelectedItem);
            _baitCrudService.SaveItem(SelectedItem);
            SelectedItem = null;
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
            ApplyFilter();
        }

        #endregion
    }
}
