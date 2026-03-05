using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Services.Business;

/// <summary>
/// Сервис для управления бэкапами рецептов прикормок
/// </summary>
public class BaitRecipesBackupService
{
    private readonly string _backupDir;
    private readonly int _maxBackupCount = 5;

    public BaitRecipesBackupService()
    {
        _backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");
        if (!Directory.Exists(_backupDir))
        {
            Directory.CreateDirectory(_backupDir);
        }
    }

    /// <summary>
    /// Создаёт бэкап рецептов с текущей датой
    /// </summary>
    public void CreateBackup(ObservableCollection<BaitRecipeModel> recipes)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var fileName = $"baitrecipes_{timestamp}.json";
            var filePath = Path.Combine(_backupDir, fileName);

            // Сериализуем в список для сохранения
            var list = recipes.ToList();
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            File.WriteAllText(filePath, json);

            // Удаляем старые бэкапы, оставляем только 5 последних
            CleanupOldBackups();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка создания бэкапа: {ex.Message}");
        }
    }

    /// <summary>
    /// Возвращает список последних бэкапов (от новых к старым)
    /// </summary>
    public List<BackupFileInfo> GetAvailableBackups()
    {
        try
        {
            if (!Directory.Exists(_backupDir))
                return new List<BackupFileInfo>();

            var files = Directory.GetFiles(_backupDir, "baitrecipes_*.json")
                .Select(f => new BackupFileInfo
                {
                    FilePath = f,
                    FileName = Path.GetFileName(f),
                    CreatedDate = File.GetCreationTime(f)
                })
                .OrderByDescending(f => f.CreatedDate)
                .Take(_maxBackupCount)
                .ToList();

            return files;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка получения списка бэкапов: {ex.Message}");
            return new List<BackupFileInfo>();
        }
    }

    /// <summary>
    /// Загружает рецепты из указанного файла бэкапа
    /// </summary>
    public ObservableCollection<BaitRecipeModel> LoadFromBackup(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return new ObservableCollection<BaitRecipeModel>();

            var json = File.ReadAllText(filePath);
            var list = JsonSerializer.Deserialize<List<BaitRecipeModel>>(json);

            return list != null ? new ObservableCollection<BaitRecipeModel>(list) : new ObservableCollection<BaitRecipeModel>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки бэкапа: {ex.Message}");
            return new ObservableCollection<BaitRecipeModel>();
        }
    }

    /// <summary>
    /// Сохраняет рецепты в указанный файл (для ручного экспорта)
    /// </summary>
    public void SaveCopy(ObservableCollection<BaitRecipeModel> recipes, string filePath)
    {
        try
        {
            var list = recipes.ToList();
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка сохранения копии: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Удаляет старые бэкапы, оставляя только maxBackupCount последних
    /// </summary>
    private void CleanupOldBackups()
    {
        try
        {
            var files = Directory.GetFiles(_backupDir, "baitrecipes_*.json")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToList();

            // Удаляем всё, что старше 5 последних
            for (int i = _maxBackupCount; i < files.Count; i++)
            {
                files[i].Delete();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка очистки старых бэкапов: {ex.Message}");
        }
    }
}

/// <summary>
/// Информация о файле бэкапа
/// </summary>
public class BackupFileInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string DisplayName => $"{CreatedDate:dd.MM.yyyy HH:mm}";
}
