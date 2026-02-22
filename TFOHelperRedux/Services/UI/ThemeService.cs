using System;
using System.Windows;
using System.Windows.Media;
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

        // Динамически загружаем файл темы
        Application.Current.Dispatcher.Invoke(() =>
        {
            LoadThemeFile(isDark);
            UpdateCustomColors();
        });
    }

    /// <summary>
    /// Загрузить файл темы (Light/Dark)
    /// </summary>
    private void LoadThemeFile(bool isDark)
    {
        var resources = Application.Current.Resources;
        var themePath = isDark ? "Themes/DarkColors.xaml" : "Themes/LightColors.xaml";

        // Находим существующий словарь с темой и заменяем его
        ResourceDictionary? themeDict = null;
        foreach (var dict in resources.MergedDictionaries)
        {
            if (dict.Source?.OriginalString.Contains("Themes/LightColors.xaml") == true ||
                dict.Source?.OriginalString.Contains("Themes/DarkColors.xaml") == true)
            {
                themeDict = dict;
                break;
            }
        }

        if (themeDict != null)
        {
            var index = resources.MergedDictionaries.IndexOf(themeDict);
            resources.MergedDictionaries[index] = new ResourceDictionary
            {
                Source = new Uri(themePath, UriKind.Relative)
            };
        }
    }

    /// <summary>
    /// Обновить кастомные цвета для темы
    /// </summary>
    private void UpdateCustomColors()
    {
        var resources = Application.Current.Resources;

        // Обновляем Brush из Color ресурсов, которые загрузились из файла темы
        UpdateBrushFromColor(resources, "AppBackground", "AppBackgroundColor");
        UpdateBrushFromColor(resources, "PanelBackground", "PanelBackgroundColor");
        UpdateBrushFromColor(resources, "TextPrimary", "TextPrimaryColor");
        UpdateBrushFromColor(resources, "TextSecondary", "TextSecondaryColor");
        UpdateBrushFromColor(resources, "BorderBrushLight", "BorderBrushLightColor");
        UpdateBrushFromColor(resources, "PrimaryColor", "PrimaryColorValue");
        UpdateBrushFromColor(resources, "SuccessColor", "SuccessColorValue");
        UpdateBrushFromColor(resources, "DangerColor", "DangerColorValue");
        UpdateBrushFromColor(resources, "AccentLocation", "AccentLocationColor");
        UpdateBrushFromColor(resources, "AccentFeed", "AccentFeedColor");
        UpdateBrushFromColor(resources, "ButtonNavLocationBrush", "ButtonNavLocationColor");
        UpdateBrushFromColor(resources, "ButtonNavBaitBrush", "ButtonNavBaitColor");
    }

    /// <summary>
    /// Обновить Brush из Color ресурса
    /// </summary>
    private void UpdateBrushFromColor(ResourceDictionary resources, string brushKey, string colorKey)
    {
        if (resources[colorKey] is Color color)
        {
            resources[brushKey] = new System.Windows.Media.SolidColorBrush(color);
        }
    }
}
