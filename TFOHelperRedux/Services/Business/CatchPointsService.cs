using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.UI;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Services.Business;

/// <summary>
/// Сервис для управления точками лова (CatchPoints).
/// Отвечает за фильтрацию, CRUD операции, импорт/экспорт.
/// </summary>
public class CatchPointsService
{
    private readonly string _localDataDir;
    private readonly string _localCatchFile;
    private readonly IUIService _uiService;
    private readonly IDataLoadSaveService _loadSaveService;

    public CatchPointsService(IUIService uiService, IDataLoadSaveService loadSaveService)
    {
        _uiService = uiService;
        _loadSaveService = loadSaveService;
        _localDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Maps");
        _localCatchFile = Path.Combine(_localDataDir, "CatchPoints_Local.json");

        if (!Directory.Exists(_localDataDir))
            Directory.CreateDirectory(_localDataDir);
    }

    /// <summary>
    /// Загрузка точек лова из файла
    /// </summary>
    public ObservableCollection<CatchPointModel> LoadCatchPoints()
    {
        var loaded = JsonService.Load<ObservableCollection<CatchPointModel>>(_localCatchFile);
        return loaded ?? new ObservableCollection<CatchPointModel>();
    }

    /// <summary>
    /// Сохранение точек лова в файл
    /// </summary>
    public void SaveCatchPoints(ObservableCollection<CatchPointModel> catchPoints)
    {
        _loadSaveService.SaveCatchPoints(_localCatchFile, catchPoints);
    }

    /// <summary>
    /// Фильтрация точек лова на основе выбранной рыбы, карты и текущего режима
    /// </summary>
    public ObservableCollection<CatchPointModel> FilterCatchPoints(
        FishModel? selectedFish,
        MapModel? selectedMap,
        string currentMode,
        ObservableCollection<CatchPointModel> allPoints)
    {
        var points = allPoints.AsEnumerable();

        switch (currentMode)
        {
            case "Fish":
                if (selectedFish != null)
                    points = points.Where(p => p.FishIDs.Contains(selectedFish.ID));
                break;

            case "Maps":
                if (selectedMap != null)
                    points = points.Where(p => p.MapID == selectedMap.ID);
                if (selectedFish != null)
                    points = points.Where(p => p.FishIDs.Contains(selectedFish.ID));
                break;

            default:
                break;
        }

        return new ObservableCollection<CatchPointModel>(points.ToList());
    }

    /// <summary>
    /// Обновление метаданных точек (MapName, FishNames)
    /// </summary>
    public void UpdateCatchPointsMetadata(
        ObservableCollection<CatchPointModel> catchPoints,
        ObservableCollection<MapModel> maps,
        ObservableCollection<FishModel> fishes)
    {
        foreach (var point in catchPoints)
        {
            point.MapName = maps.FirstOrDefault(m => m.ID == point.MapID)?.Name ?? "—";
            point.FishNames = string.Join(", ",
                point.FishIDs?.Select(id => fishes.FirstOrDefault(f => f.ID == id)?.Name)
                ?? new[] { "—" });
        }
    }

    /// <summary>
    /// Редактирование точки лова (открывает окно)
    /// </summary>
    public void EditCatchPoint(CatchPointModel? point, CatchPointsViewModel catchPointsVm)
    {
        if (point == null)
            return;

        var wnd = new Views.EditCatchPointWindow(point);
        if (wnd.ShowDialog() == true)
        {
            var fish = DataStore.Selection.SelectedFish ?? catchPointsVm.CurrentFish;
            catchPointsVm.RefreshFilteredPoints(fish);
        }
    }

    /// <summary>
    /// Открытие окна добавления/редактирования точки лова
    /// </summary>
    public void OpenEditCatchPointWindow(CatchPointsViewModel catchPointsVm)
    {
        var wnd = new Views.EditCatchPointWindow();
        if (wnd.ShowDialog() == true)
        {
            catchPointsVm.RefreshFilteredPoints(DataStore.Selection.SelectedFish);
        }
    }

    /// <summary>
    /// Импорт точек лова из файла
    /// </summary>
    public void ImportCatchPoints(ObservableCollection<CatchPointModel> catchPoints)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Импорт точек лова"
        };

        if (dlg.ShowDialog() != true)
            return;

        try
        {
            if (!File.Exists(dlg.FileName))
            {
                _uiService.ShowWarning("Файл не найден.", "Импорт точек");
                return;
            }

            var imported = JsonService.Load<List<CatchPointModel>>(dlg.FileName);
            if (imported == null || imported.Count == 0)
            {
                _uiService.ShowWarning("Не удалось загрузить данные из файла.", "Импорт точек");
                return;
            }

            var uniqueImported = GetUniqueCatchPoints(imported);

            if (uniqueImported.Count == 0)
            {
                _uiService.ShowWarning("В файле нет валидных точек лова.", "Импорт точек");
                return;
            }

            if (catchPoints.Count > 0)
            {
                var result = _uiService.ShowMessageBox(
                    "В программе уже есть точки лова.\n\n" +
                    "Объединить новые точки с существующими?\n" +
                    "Да — объединить\n" +
                    "Нет — заменить существующие\n" +
                    "Отмена — прервать импорт",
                    "Импорт точек",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                    return;

                if (result == MessageBoxResult.No)
                {
                    catchPoints.Clear();
                    foreach (var p in uniqueImported)
                        catchPoints.Add(p);
                }
                else if (result == MessageBoxResult.Yes)
                {
                    MergeCatchPoints(catchPoints, uniqueImported);
                }
            }
            else
            {
                foreach (var p in uniqueImported)
                    catchPoints.Add(p);
            }

            SaveCatchPoints(catchPoints);
            _uiService.ShowInfo("Импорт завершён ✅", "Импорт");
        }
        catch (Exception ex)
        {
            _uiService.ShowError("Ошибка при импорте точек:\n" + ex.Message, "Импорт точек");
        }
    }

    /// <summary>
    /// Экспорт точек лова в файл
    /// </summary>
    public void ExportCatchPoints(ObservableCollection<CatchPointModel> catchPoints)
    {
        var dlg = new SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            Title = "Экспорт точек лова"
        };

        if (dlg.ShowDialog() != true)
            return;

        JsonService.Save(dlg.FileName, catchPoints);
        _uiService.ShowInfo("Точки экспортированы 💾", "Экспорт");
    }

    /// <summary>
    /// Очистка всех точек лова
    /// </summary>
    public void ClearCatchPoints(ObservableCollection<CatchPointModel> catchPoints)
    {
        if (!_uiService.ShowConfirm("Очистить все точки лова?", "Подтверждение"))
            return;

        catchPoints.Clear();
        SaveCatchPoints(catchPoints);
        _uiService.ShowInfo("Точки очищены 🗑", "Очистка");
    }

    /// <summary>
    /// Получение уникальных точек лова (без дубликатов по MapID + X + Y)
    /// </summary>
    private List<CatchPointModel> GetUniqueCatchPoints(List<CatchPointModel> points)
    {
        var unique = new List<CatchPointModel>();
        var keys = new HashSet<(int MapId, int X, int Y)>();

        foreach (var p in points)
        {
            if (p?.Coords == null)
                continue;

            var key = (p.MapID, p.Coords.X, p.Coords.Y);
            if (!keys.Add(key))
                continue;

            unique.Add(p);
        }

        return unique;
    }

    /// <summary>
    /// Объединение точек лова (добавление только новых)
    /// </summary>
    private void MergeCatchPoints(
        ObservableCollection<CatchPointModel> existing,
        List<CatchPointModel> newPoints)
    {
        var existingKeys = new HashSet<(int MapId, int X, int Y)>(
            existing.Select(p => (p.MapID, p.Coords.X, p.Coords.Y)));

        int added = 0;
        foreach (var p in newPoints)
        {
            var key = (p.MapID, p.Coords.X, p.Coords.Y);
            if (existingKeys.Contains(key))
                continue;

            existing.Add(p);
            existingKeys.Add(key);
            added++;
        }

        _uiService.ShowInfo($"Импорт завершён. Добавлено {added} новых точек.", "Импорт точек");
    }
}
