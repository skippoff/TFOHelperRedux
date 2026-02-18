using System;
using System.Windows.Input;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Services.Business;
using TFOHelperRedux.Services.Data;

namespace TFOHelperRedux.ViewModels
{
    /// <summary>
    /// ViewModel для управления навигацией между режимами приложения
    /// </summary>
    public class NavigationViewModel : BaseViewModel
    {
        #region Константы режимов

        public static class Modes
        {
            public const string Fish = "Fish";
            public const string Maps = "Maps";
            public const string Baits = "Baits";
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
                    OnPropertyChanged(nameof(BaitsSubMode));
                    OnPropertyChanged(nameof(IsFeedsMode));
                    OnPropertyChanged(nameof(IsComponentsMode));
                    OnPropertyChanged(nameof(IsDipsMode));
                    OnPropertyChanged(nameof(IsLuresMode));
                    OnBaitsSubModeChanged?.Invoke();
                }
            }
        }

        #endregion

        #region Свойства для проверки текущего режима (для UI)

        public bool IsFishMode => _currentMode == Modes.Fish;
        public bool IsMapsMode => _currentMode == Modes.Maps;
        public bool IsBaitsMode => _currentMode == Modes.Baits;

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
        public ICommand ShowFeedsCmd { get; }
        public ICommand ShowComponentsCmd { get; }
        public ICommand ShowDipsCmd { get; }
        public ICommand ShowLuresCmd { get; }

        #endregion

        #region Конструктор

        public NavigationViewModel()
        {
            ShowFishCmd = new RelayCommand(() => CurrentMode = Modes.Fish);
            ShowMapsCmd = new RelayCommand(() => CurrentMode = Modes.Maps);
            ShowBaitsCmd = new RelayCommand(() => CurrentMode = Modes.Baits);

            ShowFeedsCmd = new RelayCommand(() => BaitsSubMode = BaitsSubModes.Feeds);
            ShowComponentsCmd = new RelayCommand(() => BaitsSubMode = BaitsSubModes.FeedComponents);
            ShowDipsCmd = new RelayCommand(() => BaitsSubMode = BaitsSubModes.Dips);
            ShowLuresCmd = new RelayCommand(() => BaitsSubMode = BaitsSubModes.Lures);
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
