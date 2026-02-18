using System.Windows;

namespace TFOHelperRedux.Services;

/// <summary>
/// Интерфейс для UI-операций (MessageBox и др.)
/// </summary>
public interface IUIService
{
    void ShowInfo(string message, string title = "Информация");
    void ShowWarning(string message, string title = "Предупреждение");
    void ShowError(string message, string title = "Ошибка");
    bool ShowConfirm(string message, string title = "Подтверждение");
    MessageBoxResult ShowMessageBox(string message, string title, MessageBoxButton button, MessageBoxImage icon);
}
