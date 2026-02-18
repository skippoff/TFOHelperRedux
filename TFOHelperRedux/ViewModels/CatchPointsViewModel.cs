using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;

namespace TFOHelperRedux.ViewModels;

public class CatchPointsViewModel : BaseViewModel
{
    private readonly CatchPointsService _catchPointsService;
    private readonly IUIService _uiService;

    public ObservableCollection<CatchPointModel> FilteredPoints { get; private set; } = new();
    public bool IsFiltered => CurrentFish != null;

    private FishModel? _currentFish;
    public FishModel? CurrentFish
    {
        get => _currentFish;
        private set
        {
            _currentFish = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsFiltered));
        }
    }

    public ICommand EditCatchPointWindowCommand { get; }
    public ICommand ImportPointsCmd { get; }
    public ICommand ExportPointsCmd { get; }
    public ICommand ClearPointsCmd { get; }
    public ICommand SavePointsCmd { get; }
    public ICommand DeletePointCmd { get; }
    public ICommand EditPointCmd { get; }

    public ObservableCollection<CatchPointModel> CatchPoints { get; } = new();

    public CatchPointsViewModel(CatchPointsService catchPointsService, IUIService uiService)
    {
        _catchPointsService = catchPointsService;
        _uiService = uiService;

        // Загрузка и инициализация точек
        var loadedPoints = _catchPointsService.LoadCatchPoints();
        foreach (var point in loadedPoints)
        {
            CatchPoints.Add(point);
        }

        _catchPointsService.UpdateCatchPointsMetadata(CatchPoints, DataStore.Maps, DataStore.Fishes);

        // Инициализация команд
        EditCatchPointWindowCommand = new RelayCommand(OpenEditCatchPointWindow);
        ImportPointsCmd = new RelayCommand(ImportPoints);
        ExportPointsCmd = new RelayCommand(ExportPoints);
        ClearPointsCmd = new RelayCommand(ClearPoints);
        SavePointsCmd = new RelayCommand(SavePoints);
        DeletePointCmd = new RelayCommand(p => DeletePoint(p as CatchPointModel));
        EditPointCmd = new RelayCommand(p => EditPoint(p as CatchPointModel));
    }

    public void RefreshFilteredPoints(FishModel? selectedFish)
    {
        if (selectedFish == null)
        {
            selectedFish = CurrentFish;
        }

        FilteredPoints.Clear();
        CurrentFish = selectedFish;

        var points = _catchPointsService.FilterCatchPoints(
            selectedFish,
            DataStore.Selection.SelectedMap,
            DataStore.CurrentMode,
            CatchPoints);

        foreach (var p in points)
            FilteredPoints.Add(p);
    }

    private void DeletePoint(CatchPointModel? point)
    {
        if (point == null)
            return;

        var result = _uiService.ShowMessageBox(
            $"Удалить точку лова на {point.MapName} (X={point.Coords.X}; Y={point.Coords.Y})?",
            "Удаление точки лова",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        // Удаляем из коллекции — UI обновится мгновенно
        CatchPoints.Remove(point);
        FilteredPoints.Remove(point);

        // Сохраняем изменения
        _catchPointsService.SaveCatchPoints(CatchPoints);
    }

    private void EditPoint(CatchPointModel? point)
    {
        _catchPointsService.EditCatchPoint(point, this);
    }

    private void OpenEditCatchPointWindow()
    {
        _catchPointsService.OpenEditCatchPointWindow(this);
    }

    private void ImportPoints()
    {
        _catchPointsService.ImportCatchPoints(CatchPoints);
        RefreshCatchPoints();
    }

    private void ExportPoints()
    {
        _catchPointsService.ExportCatchPoints(CatchPoints);
    }

    private void ClearPoints()
    {
        _catchPointsService.ClearCatchPoints(CatchPoints);
        CatchPoints.Clear();
        RefreshFilteredPoints(CurrentFish);
    }

    private void SavePoints()
    {
        _catchPointsService.SaveCatchPoints(CatchPoints);
        _uiService.ShowInfo("Изменения сохранены 💾", "Сохранение");
    }

    private void RefreshCatchPoints()
    {
        CatchPoints.Clear();
        var loadedPoints = _catchPointsService.LoadCatchPoints();
        foreach (var point in loadedPoints)
        {
            CatchPoints.Add(point);
        }

        _catchPointsService.UpdateCatchPointsMetadata(CatchPoints, DataStore.Maps, DataStore.Fishes);
    }
}
