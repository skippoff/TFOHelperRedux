using System;
using System.Windows;

namespace TFOHelperRedux.Services.UI;

/// <summary>
/// Сервис для UI-операций (MessageBox и др.)
/// </summary>
public class UIService : IUIService
{
    public void ShowInfo(string message, string title = "Информация")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowWarning(string message, string title = "Предупреждение")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public void ShowError(string message, string title = "Ошибка")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public bool ShowConfirm(string message, string title = "Подтверждение")
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public MessageBoxResult ShowMessageBox(string message, string title, MessageBoxButton button, MessageBoxImage icon)
    {
        return MessageBox.Show(message, title, button, icon);
    }

    /// <summary>
    /// Показать окно модально с callback после закрытия
    /// </summary>
    public static void ShowWindowModal(Window window, Action? onClose = null)
    {
        var owner = Application.Current.MainWindow;
        if (owner != null)
            window.Owner = owner;

        window.ShowDialog();
        onClose?.Invoke();
    }

    /// <summary>
    /// Показать сообщение (обёртка над MessageBox)
    /// </summary>
    public static void ShowMessage(string message, string title = "Информация", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
    {
        MessageBox.Show(message, title, button, icon);
    }
}
