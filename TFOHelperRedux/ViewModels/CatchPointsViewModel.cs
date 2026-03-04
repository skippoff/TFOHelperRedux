using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services.Business;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.UI;
using TFOHelperRedux.Views;

namespace TFOHelperRedux.ViewModels;

public class CatchPointsViewModel : BaseViewModel
{
    private readonly CatchPointsService _catchPointsService;
    private readonly IUIService _uiService;
    private bool _isRefreshing;

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
    public ICommand OpenMapCmd { get; }
    public ICommand CopyCoordsCmd { get; }

    // Используем единую коллекцию из DataStore
    public ObservableCollection<CatchPointModel> CatchPoints => DataStore.CatchPoints;

    public CatchPointsViewModel(CatchPointsService catchPointsService, IUIService uiService)
    {
        _catchPointsService = catchPointsService;
        _uiService = uiService;

        // Обновляем метаданные точек после загрузки DataStore
        _catchPointsService.UpdateCatchPointsMetadata(CatchPoints, DataStore.Maps, DataStore.Fishes);

        // Инициализация команд
        EditCatchPointWindowCommand = new RelayCommand(OpenEditCatchPointWindow);
        ImportPointsCmd = new RelayCommand(ImportPoints);
        ExportPointsCmd = new RelayCommand(ExportPoints);
        ClearPointsCmd = new RelayCommand(ClearPoints);
        SavePointsCmd = new RelayCommand(SavePoints);
        DeletePointCmd = new RelayCommand(p => DeletePoint(p as CatchPointModel));
        EditPointCmd = new RelayCommand(p => EditPoint(p as CatchPointModel));
        OpenMapCmd = new RelayCommand(p => OpenMap(p as CatchPointModel));
        CopyCoordsCmd = new RelayCommand(p => CopyCoords(p as CatchPointModel));
    }

    public void RefreshFilteredPoints(FishModel? selectedFish)
    {
        // Защита от повторных вызовов
        if (_isRefreshing)
            return;

        if (selectedFish == null)
        {
            selectedFish = CurrentFish;
        }

        _isRefreshing = true;
        try
        {
            CurrentFish = selectedFish;

            var points = _catchPointsService.FilterCatchPoints(
                selectedFish,
                DataStore.Selection.SelectedMap,
                DataStore.CurrentMode,
                CatchPoints);

            // Эффективное обновление коллекции
            UpdateCollection(points, FilteredPoints);

            // Явно триггерим обновление UI
            OnPropertyChanged(nameof(FilteredPoints));
            OnPropertyChanged(nameof(IsFiltered));
            
            // Автоматически выбираем первую точку лова для выбранной рыбы
            // Если текущая точка не принадлежит новой рыбе — выбираем первую из отфильтрованных
            var newCatchPoint = FilteredPoints.FirstOrDefault(cp => 
                cp.FishIDs != null && cp.FishIDs.Contains(selectedFish.ID));
            
            if (newCatchPoint != null)
            {
                DataStore.Selection.SelectedCatchPoint = newCatchPoint;
            }
            else if (FilteredPoints.Count > 0)
            {
                DataStore.Selection.SelectedCatchPoint = FilteredPoints[0];
            }
            else
            {
                // Если у рыбы нет точек лова — очищаем выбор
                DataStore.Selection.SelectedCatchPoint = null;
            }
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    /// <summary>
    /// Эффективное обновление коллекции с минимальными уведомленияциями
    /// </summary>
    private static void UpdateCollection(IEnumerable<CatchPointModel> newItems, ObservableCollection<CatchPointModel> collection)
    {
        var newList = newItems as IList<CatchPointModel> ?? newItems.ToList();
        var existingSet = new HashSet<CatchPointModel>(collection);
        var newSet = new HashSet<CatchPointModel>(newList);

        // Удаляем элементы, которых больше нет
        for (int i = collection.Count - 1; i >= 0; i--)
        {
            if (!newSet.Contains(collection[i]))
                collection.RemoveAt(i);
        }

        // Добавляем новые элементы
        foreach (var item in newList)
        {
            if (!existingSet.Contains(item))
                collection.Add(item);
        }
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
        RefreshFilteredPoints(CurrentFish);
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

    private void OpenMap(CatchPointModel? point)
    {
        if (point == null)
            return;

        var map = DataStore.Maps.FirstOrDefault(m => m.ID == point.MapID);
        if (map == null)
        {
            _uiService.ShowError("Карта не найдена для этой точки.", "Ошибка");
            return;
        }

        // Проверяем, есть ли уже открытое окно карты
        var mapWindow = Application.Current.Windows
            .OfType<MapPreviewWindow>()
            .FirstOrDefault(w => w.IsLoaded);

        if (mapWindow == null || !mapWindow.IsLoaded)
        {
            mapWindow = new MapPreviewWindow(map, point);
            mapWindow.Show();
        }
        else
        {
            mapWindow.UpdatePoint(map, point);
            if (!mapWindow.IsVisible)
                mapWindow.Show();

            mapWindow.Activate();
        }
    }

    private void CopyCoords(CatchPointModel? point)
    {
        if (point == null)
            return;

        // Форматируем координаты как "X Y" без дополнения нулями
        var coordsString = $"{point.Coords.X} {point.Coords.Y}";
        Clipboard.SetText(coordsString);
    }
}
