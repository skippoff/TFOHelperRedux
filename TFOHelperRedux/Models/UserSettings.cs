using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace TFOHelperRedux.Models;

/// <summary>
/// Настройки пользователя
/// </summary>
public class UserSettings : INotifyPropertyChanged
{
    private string _nickName = "Anonymous";
    private bool _showNickNameInExport = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Никнейм пользователя (отображается в экспортированных файлах)
    /// </summary>
    [JsonPropertyName("nickName")]
    public string NickName
    {
        get => _nickName;
        set
        {
            if (_nickName != value)
            {
                _nickName = string.IsNullOrWhiteSpace(value) ? "Anonymous" : value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Показывать никнейм в экспортированных файлах точек лова
    /// </summary>
    [JsonPropertyName("showNickNameInExport")]
    public bool ShowNickNameInExport
    {
        get => _showNickNameInExport;
        set
        {
            if (_showNickNameInExport != value)
            {
                _showNickNameInExport = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Версия схемы настроек (для будущей миграции)
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; } = 1;
}
