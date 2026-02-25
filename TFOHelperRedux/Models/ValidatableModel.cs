using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TFOHelperRedux.Models;

/// <summary>
/// Базовый класс для моделей с поддержкой валидации
/// </summary>
public abstract class ValidatableModel : INotifyPropertyChanged, IDataErrorInfo
{
    private readonly Dictionary<string, string> _errors = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Устанавливает свойство и уведомляет об изменении
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        
        // Перепроверяем валидацию после изменения
        Validate(propertyName);
        
        return true;
    }

    /// <summary>
    /// Уведомляет об изменении свойства
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Также уведомляем IDataErrorInfo
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasErrors)));
    }

    /// <summary>
    /// Публичный метод для уведомления об изменении свойства (для использования вне класса)
    /// </summary>
    public void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(propertyName);
    }

    /// <summary>
    /// Проверяет валидность свойства
    /// </summary>
    protected virtual void Validate(string? propertyName = null)
    {
        // Переопределяется в наследниках
    }

    /// <summary>
    /// Добавляет ошибку валидации
    /// </summary>
    protected void AddError(string propertyName, string error)
    {
        _errors[propertyName] = error;
        OnPropertyChanged(propertyName);
    }

    /// <summary>
    /// Удаляет ошибку валидации
    /// </summary>
    protected void RemoveError(string propertyName)
    {
        if (_errors.Remove(propertyName))
            OnPropertyChanged(propertyName);
    }

    /// <summary>
    /// Проверяет наличие ошибок
    /// </summary>
    public bool HasErrors => _errors.Count > 0;

    /// <summary>
    /// Получает все ошибки
    /// </summary>
    public IEnumerable<string> GetAllErrors() => _errors.Values;

    #region IDataErrorInfo

    /// <summary>
    /// Ошибка объекта (не используется в WPF, но требуется интерфейсом)
    /// </summary>
    public string Error => string.Join("; ", _errors.Values);

    /// <summary>
    /// Ошибка для конкретного свойства
    /// </summary>
    public string this[string columnName]
    {
        get
        {
            Validate(columnName);
            return _errors.TryGetValue(columnName, out var error) ? error : string.Empty;
        }
    }

    #endregion
}
