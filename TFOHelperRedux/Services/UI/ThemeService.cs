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
            // Тёмная тема
            resources["AppBackground"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(30, 30, 30));
            resources["PanelBackground"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(45, 45, 45));
            resources["TextPrimary"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 255, 255));
            resources["TextSecondary"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(180, 180, 180));
            resources["BorderBrushLight"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(60, 60, 60));
            
            // Адаптация акцентных цветов для тёмной темы (более яркие)
            resources["PrimaryColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(66, 165, 245));
            resources["SuccessColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(102, 187, 106));
            resources["DangerColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(239, 83, 80));
            resources["AccentLocation"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0, 188, 212));
            resources["AccentFeed"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 167, 77));
            resources["ButtonNavLocationBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0, 188, 212));
            resources["ButtonNavBaitBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 152, 0));
        }
        else
        {
            // Светлая тема
            resources["AppBackground"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(112, 128, 144)); // SlateGray
            resources["PanelBackground"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(240, 248, 255)); // AliceBlue
            resources["TextPrimary"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(33, 33, 33));
            resources["TextSecondary"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(117, 117, 117));
            resources["BorderBrushLight"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(224, 224, 224));
            
            // Оригинальные цвета для светлой темы
            resources["PrimaryColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(33, 150, 243));
            resources["SuccessColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(76, 175, 80));
            resources["DangerColor"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(229, 57, 53));
            resources["AccentLocation"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0, 183, 179));
            resources["AccentFeed"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 165, 46));
            resources["ButtonNavLocationBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0, 150, 136));
            resources["ButtonNavBaitBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(245, 124, 0));
        }
    }
}
