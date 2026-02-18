namespace TFOHelperRedux.Services;

/// <summary>
/// Результат операции
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

    public void ShowMessageBox(IUIService uiService)
    {
        if (string.IsNullOrEmpty(Message))
            return;

        if (IsSuccess)
            uiService.ShowInfo(Message);
        else
            uiService.ShowWarning(Message);
    }
}
