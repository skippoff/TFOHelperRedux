using TFOHelperRedux.Services;

namespace TFOHelperRedux;

/// <summary>
/// Глобальный контекст пользователя (статический доступ к настройкам)
/// </summary>
public static class UserContext
{
    private static SettingsService? _settingsService;
    private static Models.UserSettings? _settings;

    /// <summary>
    /// Инициализация контекста пользователя (должна вызываться при старте приложения)
    /// </summary>
    public static void Initialize()
    {
        _settingsService = new SettingsService();
        _settings = _settingsService.LoadSettings();
    }

    /// <summary>
    /// Никнейм текущего пользователя
    /// </summary>
    public static string NickName => _settings?.NickName ?? "Anonymous";

    /// <summary>
    /// Показывать никнейм в экспорте
    /// </summary>
    public static bool ShowNickNameInExport => _settings?.ShowNickNameInExport ?? true;

    /// <summary>
    /// Получить MadeBy для точки лова (с учётом настройки отображения)
    /// </summary>
    public static string GetMadeBy()
    {
        return ShowNickNameInExport ? NickName : "Anonymous";
    }

    /// <summary>
    /// Обновить настройки (для использования после сохранения в UI)
    /// </summary>
    public static void Refresh()
    {
        if (_settingsService != null)
        {
            _settings = _settingsService.LoadSettings();
        }
    }
}
