using System;
using System.Windows;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

namespace TFOHelperRedux.Services.UI;

/// <summary>
/// Сервис для управления темой приложения (светлая/тёмная)
/// </summary>
public class ThemeService
{
    private bool _isDarkTheme = false;

    /// <summary>
    /// Текущая тема (true = тёмная, false = светлая)
    /// </summary>
    public bool IsDarkTheme => _isDarkTheme;

    /// <summary>
    /// Переключить тему
    /// </summary>
    public void ToggleTheme()
    {
        _isDarkTheme = !_isDarkTheme;
        ApplyTheme(_isDarkTheme);
    }

    /// <summary>
    /// Установить тему
    /// </summary>
    public void SetTheme(bool isDark)
    {
        if (_isDarkTheme != isDark)
        {
            _isDarkTheme = isDark;
            ApplyTheme(isDark);
        }
    }

    /// <summary>
    /// Применить тему к приложению
    /// </summary>
    private void ApplyTheme(bool isDark)
    {
        var resources = Application.Current.Resources;
        var theme = resources.GetTheme();
        
        theme.SetBaseTheme(isDark ? BaseTheme.Dark : BaseTheme.Light);
        resources.SetTheme(theme);

        // Обновляем кастомные цвета
        Application.Current.Dispatcher.Invoke(() =>
        {
            UpdateCustomColors(isDark);
        });
    }

    /// <summary>
    /// Обновить кастомные цвета для темы
    /// </summary>
    private void UpdateCustomColors(bool isDark)
    {
        var resources = Application.Current.Resources;

        if (isDark)
        {
            // Тёмная тема — загружаем цвета из DarkColors.xaml
            resources["AppBackground"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(30, 30, 46)); // #1E1E2E
            resources["PanelBackground"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(43, 43, 60)); // #2B2B3C
            resources["TextPrimary"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(224, 224, 224)); // #E0E0E0
            resources["TextSecondary"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(160, 160, 176)); // #A0A0B0
            resources["BorderBrushLight"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(60, 60, 80)); // #3C3C50

            // Адаптация акцентных цветов для тёмной темы (более яркие)
            resources["PrimaryColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(100, 181, 246)); // #64B5F6
            resources["SuccessColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(102, 187, 106)); // #66BB6A
            resources["DangerColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(239, 83, 80)); // #EF5350
            resources["AccentExperimental"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 183, 77)); // #FFB74D
            resources["AccentLocation"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(77, 208, 225)); // #4DD0E1
            resources["AccentFeed"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 183, 77)); // #FFB74D
            resources["ButtonImportBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(102, 187, 106)); // #66BB6A
            resources["ButtonExportBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(100, 181, 246)); // #64B5F6
            resources["ButtonDeleteBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(239, 83, 80)); // #EF5350
            resources["ButtonEditBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(198, 216, 46)); // #C6D82E
            resources["ButtonNavLocationBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(38, 166, 154)); // #26A69A
            resources["ButtonNavBaitBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 140, 0)); // #FF8C00
        }
        else
        {
            // Светлая тема — загружаем цвета из LightColors.xaml
            resources["AppBackground"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(112, 128, 144)); // SlateGray
            resources["PanelBackground"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(240, 248, 255)); // AliceBlue
            resources["TextPrimary"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(33, 33, 33)); // #212121
            resources["TextSecondary"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(102, 102, 102)); // #666666
            resources["BorderBrushLight"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(221, 221, 221)); // #DDDDDD

            // Оригинальные цвета для светлой темы
            resources["PrimaryColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(33, 150, 243)); // #2196F3
            resources["SuccessColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(76, 175, 80)); // #4CAF50
            resources["DangerColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(229, 57, 53)); // #E53935
            resources["AccentExperimental"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 152, 0)); // #FF9800
            resources["AccentLocation"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0, 183, 179)); // #00B7B3
            resources["AccentFeed"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 165, 46)); // #FFA52E
            resources["ButtonImportBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(76, 175, 80)); // #4CAF50
            resources["ButtonExportBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(33, 150, 243)); // #2196F3
            resources["ButtonDeleteBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(229, 115, 115)); // #E57373
            resources["ButtonEditBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(173, 187, 42)); // #ADBB2A
            resources["ButtonNavLocationBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(127, 255, 212)); // #7FFFD4
            resources["ButtonNavBaitBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 140, 0)); // #FF8C00
        }
    }
}
