using System.Windows;

namespace TFOHelperRedux.Services;

/// <summary>
/// Результат операции привязки/удаления
/// </summary>
public class ServiceResult
{
    public bool IsSuccess { get; private set; }
    public string? Message { get; private set; }

    public static ServiceResult Success(string message) => new()
    {
        IsSuccess = true,
        Message = message
    };

    public static ServiceResult Failure(string message) => new()
    {
        IsSuccess = false,
        Message = message
    };

    public void ShowMessageBox()
    {
        if (string.IsNullOrEmpty(Message))
            return;

        var icon = IsSuccess ? MessageBoxImage.Information : MessageBoxImage.Warning;
        var title = IsSuccess ? "Успешно" : "Ошибка";
        MessageBox.Show(Message, title, MessageBoxButton.OK, icon);
    }
}
