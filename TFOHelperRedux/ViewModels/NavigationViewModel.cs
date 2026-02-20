using System;
using System.Windows.Input;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Services.Business;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.DI;
using TFOHelperRedux.Services.UI;

namespace TFOHelperRedux.ViewModels
{
    /// <summary>
    /// ViewModel для управления навигацией между режимами приложения
    /// </summary>
    public class NavigationViewModel : BaseViewModel
    {
        private readonly ThemeService _themeService;
        #region Константы режимов

        public static class Modes
        {
            public const string Fish = "Fish";
            public const string Maps = "Maps";
            public const string Baits = "Baits";
            public const string FishFeeds = "FishFeeds";
        }

        public static class BaitsSubModes
        {
            public const string Feeds = "Feeds";
            public const string FeedComponents = "FeedComponents";
            public const string Dips = "Dips";
            public const string Lures = "Lures";
        }

        #endregion

        #region Поля

        private string _currentMode = "Maps";
        private string _baitsSubMode = BaitsSubModes.Feeds;

        #endregion

        #region Свойства режимов

        /// <summary>
        /// Текущий режим приложения (Fish / Maps / Baits)
        /// </summary>
        public string CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode != value)
                {
                    _currentMode = value;
                    OnPropertyChanged(nameof(CurrentMode));
                    OnPropertyChanged(nameof(IsFishMode));
                    OnPropertyChanged(nameof(IsMapsMode));
                    OnPropertyChanged(nameof(IsBaitsMode));
                    OnModeChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// Под-режим для Baits (Feeds / FeedComponents / Dips / Lures)
        /// </summary>
        public string BaitsSubMode
        {
            get => _baitsSubMode;
            set
            {
                if (_baitsSubMode != value)
                {
                    _baitsSubMode = value;
                    // Уведомляем только об изменении под-режима
                    OnPropertyChanged(nameof(BaitsSubMode));
                    // Не уведомляем все Is*Mode свойства — они используются редко
                    // Вызываем событие для обновления UI
                    OnBaitsSubModeChanged?.Invoke();
                }
            }
        }

        #endregion

        #region Свойства для проверки текущего режима (для UI)

        public bool IsFishMode => _currentMode == Modes.Fish;
        public bool IsMapsMode => _currentMode == Modes.Maps;
        public bool IsBaitsMode => _currentMode == Modes.Baits;
        public bool IsFishFeedsMode => _currentMode == Modes.FishFeeds;

        public bool IsFeedsMode => _baitsSubMode == BaitsSubModes.Feeds;
        public bool IsComponentsMode => _baitsSubMode == BaitsSubModes.FeedComponents;
        public bool IsDipsMode => _baitsSubMode == BaitsSubModes.Dips;
        public bool IsLuresMode => _baitsSubMode == BaitsSubModes.Lures;

        #endregion

        #region Делегаты обратного вызова

        public Action? OnModeChanged { get; set; }
        public Action? OnBaitsSubModeChanged { get; set; }

        #endregion

        #region Команды навигации

        public ICommand ShowFishCmd { get; }
        public ICommand ShowMapsCmd { get; }
        public ICommand ShowBaitsCmd { get; }
        public ICommand ShowFishFeedsCmd { get; }
        public ICommand ShowFeedsCmd { get; }
        public ICommand ShowComponentsCmd { get; }
        public ICommand ShowDipsCmd { get; }
        public ICommand ShowLuresCmd { get; }
        public ICommand ToggleThemeCmd { get; }

        #endregion

        #region Конструктор

        public NavigationViewModel()
        {
            _themeService = ServiceContainer.GetService<ThemeService>();
            
            ShowFishCmd = new RelayCommand(() => CurrentMode = Modes.Fish);
            ShowMapsCmd = new RelayCommand(() => CurrentMode = Modes.Maps);
            ShowBaitsCmd = new RelayCommand(() => CurrentMode = Modes.Baits);
            ShowFishFeedsCmd = new RelayCommand(() => CurrentMode = Modes.FishFeeds);

            ShowFeedsCmd = new RelayCommand(() => BaitsSubMode = BaitsSubModes.Feeds);
            ShowComponentsCmd = new RelayCommand(() => BaitsSubMode = BaitsSubModes.FeedComponents);
            ShowDipsCmd = new RelayCommand(() => BaitsSubMode = BaitsSubModes.Dips);
            ShowLuresCmd = new RelayCommand(() => BaitsSubMode = BaitsSubModes.Lures);
            ToggleThemeCmd = new RelayCommand(() => _themeService.ToggleTheme());
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Переключает режим на Fish
        /// </summary>
        public void NavigateToFish() => CurrentMode = Modes.Fish;

        /// <summary>
        /// Переключает режим на Maps
        /// </summary>
        public void NavigateToMaps() => CurrentMode = Modes.Maps;

        /// <summary>
        /// Переключает режим на Baits
        /// </summary>
        public void NavigateToBaits() => CurrentMode = Modes.Baits;

        #endregion
    }
}
