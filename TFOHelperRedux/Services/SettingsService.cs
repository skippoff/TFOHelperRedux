using System;
using System.IO;
using Serilog;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;

namespace TFOHelperRedux.Services;

/// <summary>
/// Сервис для управления настройками пользователя
/// </summary>
public class SettingsService
{
    private static readonly ILogger _log = Log.ForContext<SettingsService>();
    private readonly string _settingsPath;
    private UserSettings? _settings;

    public SettingsService()
    {
        _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json");
        _log.Debug("Путь к файлу настроек: {Path}", _settingsPath);
    }

    /// <summary>
    /// Загрузка настроек из файла
    /// </summary>
    public UserSettings LoadSettings()
    {
        _log.Debug("Загрузка настроек из {Path}", _settingsPath);

        try
        {
            if (!File.Exists(_settingsPath))
            {
                _log.Debug("Файл настроек не найден, создаются настройки по умолчанию");
                _settings = new UserSettings();
                SaveSettings(_settings);
                return _settings;
            }

            var loaded = JsonService.Load<UserSettings>(_settingsPath);
            _settings = loaded ?? new UserSettings();

            // Миграция настроек при необходимости
            MigrateSettings(_settings);

            _log.Debug("Настройки загружены: NickName={NickName}, ShowNickName={ShowNickName}", 
                _settings.NickName, _settings.ShowNickNameInExport);

            return _settings;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Ошибка при загрузке настроек");
            _settings = new UserSettings();
            return _settings;
        }
    }

    /// <summary>
    /// Сохранение настроек в файл
    /// </summary>
    public void SaveSettings(UserSettings settings)
    {
        _log.Debug("Сохранение настроек в {Path}", _settingsPath);

        try
        {
            JsonService.Save(_settingsPath, settings);
            _settings = settings;
            _log.Debug("Настройки сохранены");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Ошибка при сохранении настроек");
            throw;
        }
    }

    /// <summary>
    /// Получение текущих настроек (ленивая загрузка)
    /// </summary>
    public UserSettings GetSettings()
    {
        return _settings ?? LoadSettings();
    }

    /// <summary>
    /// Обновление никнейма
    /// </summary>
    public void UpdateNickName(string nickName)
    {
        var settings = GetSettings();
        settings.NickName = nickName;
        SaveSettings(settings);
        _log.Information("Никнейм обновлён: {NickName}", nickName);
    }

    /// <summary>
    /// Обновление настройки отображения никнейма
    /// </summary>
    public void UpdateShowNickName(bool show)
    {
        var settings = GetSettings();
        settings.ShowNickNameInExport = show;
        SaveSettings(settings);
        _log.Information("ShowNickNameInExport обновлён: {Show}", show);
    }

    /// <summary>
    /// Миграция настроек между версиями
    /// </summary>
    private void MigrateSettings(UserSettings settings)
    {
        // Пока миграция не требуется, версия схемы = 1
        if (settings.SchemaVersion < 1)
        {
            _log.Debug("Миграция настроек с версии {Version} до текущей", settings.SchemaVersion);
            settings.SchemaVersion = 1;
            SaveSettings(settings);
        }
    }
}
