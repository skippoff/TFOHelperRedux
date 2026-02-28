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
    private readonly PaletteHelper _paletteHelper;
    private bool _isDarkTheme = false;

    /// <summary>
    /// Текущая тема (true = тёмная, false = светлая)
    /// </summary>
    public bool IsDarkTheme => _isDarkTheme;

    public ThemeService()
    {
        _paletteHelper = new PaletteHelper();
    }

    /// <summary>
    /// Инициализировать начальную тему (вызывать при запуске приложения)
    /// </summary>
    public void InitializeTheme()
    {
        ApplyTheme(_isDarkTheme);
    }

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
        Application.Current.Dispatcher.Invoke(() =>
        {
            var resources = Application.Current.Resources;

            // Сначала меняем наши цвета
            var mergedDicts = resources.MergedDictionaries;
            ResourceDictionary colorsDict = null;
            int colorsDictIndex = -1;

            System.Diagnostics.Debug.WriteLine($"[ThemeService] Всего словарей ДО: {mergedDicts.Count}");
            for (int i = 0; i < mergedDicts.Count; i++)
            {
                var dict = mergedDicts[i];
                var source = dict.Source?.ToString() ?? "inline";
                System.Diagnostics.Debug.WriteLine($"[ThemeService]   [{i}] {source}");
                
                if (dict.Contains("AppBackgroundColor"))
                {
                    colorsDict = dict;
                    colorsDictIndex = i;
                }
            }

            System.Diagnostics.Debug.WriteLine($"[ThemeService] Найден словарь цветов: {colorsDict != null}, индекс: {colorsDictIndex}");

            if (colorsDict != null && colorsDictIndex >= 0)
            {
                mergedDicts.RemoveAt(colorsDictIndex);
            }

            var colorsUri = new Uri(
                isDark ? "Themes/DarkColors.xaml" : "Themes/LightColors.xaml",
                UriKind.Relative);

            var newColorsDict = new ResourceDictionary { Source = colorsUri };
            
            // Вставляем на то же место, где был старый словарь
            if (colorsDictIndex >= 0 && colorsDictIndex < mergedDicts.Count)
            {
                mergedDicts.Insert(colorsDictIndex, newColorsDict);
            }
            else
            {
                mergedDicts.Add(newColorsDict);
            }

            System.Diagnostics.Debug.WriteLine($"[ThemeService] Добавлен: {colorsUri} на позицию {colorsDictIndex}");
            System.Diagnostics.Debug.WriteLine($"[ThemeService] Всего словарей ПОСЛЕ: {mergedDicts.Count}");

            // Проверяем, что цвета загрузились
            if (resources.Contains("AppBackgroundColor"))
            {
                var color = (System.Windows.Media.Color)resources["AppBackgroundColor"];
                System.Diagnostics.Debug.WriteLine($"[ThemeService] AppBackgroundColor = #{color.R:X2}{color.G:X2}{color.B:X2}");
            }

            // Обновляем кисти вручную (DynamicResource не всегда обновляется)
            if (resources.Contains("AppBackgroundColor"))
            {
                var color = (System.Windows.Media.Color)resources["AppBackgroundColor"];
                resources["AppBackground"] = new System.Windows.Media.SolidColorBrush(color);
            }
            if (resources.Contains("PanelBackgroundColor"))
            {
                var color = (System.Windows.Media.Color)resources["PanelBackgroundColor"];
                resources["PanelBackground"] = new System.Windows.Media.SolidColorBrush(color);
            }
            if (resources.Contains("TextPrimaryColor"))
            {
                var color = (System.Windows.Media.Color)resources["TextPrimaryColor"];
                resources["TextPrimary"] = new System.Windows.Media.SolidColorBrush(color);
            }
            if (resources.Contains("TextSecondaryColor"))
            {
                var color = (System.Windows.Media.Color)resources["TextSecondaryColor"];
                resources["TextSecondary"] = new System.Windows.Media.SolidColorBrush(color);
            }
            if (resources.Contains("BorderBrushLightColor"))
            {
                var color = (System.Windows.Media.Color)resources["BorderBrushLightColor"];
                resources["BorderBrushLight"] = new System.Windows.Media.SolidColorBrush(color);
            }
            if (resources.Contains("ToolTipBackgroundColor"))
            {
                var color = (System.Windows.Media.Color)resources["ToolTipBackgroundColor"];
                resources["ToolTipBackgroundBrush"] = new System.Windows.Media.SolidColorBrush(color);
            }
            if (resources.Contains("ToolTipTextColor"))
            {
                var color = (System.Windows.Media.Color)resources["ToolTipTextColor"];
                resources["ToolTipForegroundBrush"] = new System.Windows.Media.SolidColorBrush(color);
            }
            if (resources.Contains("ToolTipBorderColor"))
            {
                var color = (System.Windows.Media.Color)resources["ToolTipBorderColor"];
                resources["ToolTipBorderBrush"] = new System.Windows.Media.SolidColorBrush(color);
            }

            // Проверяем наши кисти после обновления
            if (resources.Contains("AppBackground"))
            {
                var brush = (System.Windows.Media.SolidColorBrush)resources["AppBackground"];
                System.Diagnostics.Debug.WriteLine($"[ThemeService] AppBackground кисть = #{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}");
            }

            // Потом Material Design через PaletteHelper
            try
            {
                var theme = _paletteHelper.GetTheme();
                theme.SetBaseTheme(isDark ? BaseTheme.Dark : BaseTheme.Light);
                _paletteHelper.SetTheme(theme);
                System.Diagnostics.Debug.WriteLine($"[ThemeService] Material Design тема применена: {(isDark ? "Dark" : "Light")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeService] Ошибка при смене Material Design темы: {ex.Message}");
            }

            System.Diagnostics.Debug.WriteLine($"[ThemeService] Тема применена: {(isDark ? "Dark" : "Light")}");
        });
    }
}
