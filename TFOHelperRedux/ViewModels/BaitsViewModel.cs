using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
        
        // Дебаунс для поиска (защита от лагов при быстром вводе)
        private readonly DispatcherTimer _debounceTimer;
        
        // Кэш результатов поиска для мгновенного повторного поиска
        private readonly Dictionary<string, List<IItemModel>> _searchCache;

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
                    
                    // Перезапуск таймера дебаунса
                    _debounceTimer.Stop();
                    _debounceTimer.Start();
                    
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

            // Инициализация таймера дебаунса (400 мс — баланс между отзывчивостью и производительностью)
            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(400)
            };
            _debounceTimer.Tick += DebounceTimer_Tick;
            
            // Инициализация кэша поиска
            _searchCache = new Dictionary<string, List<IItemModel>>();

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

        #region Обработчик таймера дебаунса

        /// <summary>
        /// Обработчик таймера — вызывается через 400 мс после последнего изменения поиска
        /// </summary>
        private void DebounceTimer_Tick(object? sender, EventArgs e)
        {
            _debounceTimer.Stop();
            ApplyFilterWithCache();
        }

        /// <summary>
        /// Фильтрация с кэшированием результатов поиска
        /// </summary>
        private void ApplyFilterWithCache()
        {
            // Если поиск пустой — загружаем все элементы без кэширования
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadCurrentCategory();
                _searchCache.Clear(); // Очищаем кэш при сбросе поиска
                return;
            }

            // Создаём ключ кэша: категория + поисковый запрос (lowercase для регистронезависимости)
            var cacheKey = $"{_currentCategory}:{SearchText.ToLowerInvariant()}";
            
            // Проверяем кэш
            if (_searchCache.TryGetValue(cacheKey, out var cached))
            {
                // Найдено в кэше — используем закэшированный результат
                UpdateCollectionEfficient(cached, CurrentItems);
                return;
            }

            // Нет в кэше — фильтруем
            var newItems = _currentCategory switch
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

            var newList = newItems.ToList();
            
            // Кэшируем результат
            _searchCache[cacheKey] = newList;
            
            // Обновляем коллекцию
            UpdateCollectionEfficient(newList, CurrentItems);
        }

        #endregion

        #region Методы навигации по категориям

        private void SwitchCategory(CategoryType category)
        {
            _currentCategory = category;
            LoadCurrentCategory();
            ApplyFilterWithCache(); // Используем метод с кэшированием
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
            var newCategory = categoryName switch
            {
                "Feeds" => CategoryType.Feeds,
                "FeedComponents" => CategoryType.FeedComponents,
                "Dips" => CategoryType.Dips,
                "Lures" => CategoryType.Lures,
                _ => _currentCategory
            };

            if (_currentCategory == newCategory)
                return;

            _currentCategory = newCategory;

            // Эффективное обновление коллекции с кэшированием
            ApplyFilterWithCache();
        }

        private void ApplyFilterEfficient()
        {
            var newItems = _currentCategory switch
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

            // Эффективное обновление коллекции
            UpdateCollectionEfficient(newItems, CurrentItems);

            OnPropertyChanged(nameof(CurrentItems));
            SelectedItem = null;
        }

        private static void UpdateCollectionEfficient(IEnumerable<IItemModel> newItems, ObservableCollection<IItemModel> collection)
        {
            var newList = newItems.ToList();
            var newSet = new HashSet<IItemModel>(newList);
            var existingSet = new HashSet<IItemModel>(collection);

            // Удаляем элементы, которых больше нет
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (!newSet.Contains(collection[i]))
                    collection.RemoveAt(i);
            }

            // Добавляем новые элементы
            foreach (var item in newList)
            {
                if (!existingSet.Contains(item))
                    collection.Add(item);
            }
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
    }
}
