using System.Collections.ObjectModel;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using TFOHelperRedux.Helpers;


namespace TFOHelperRedux.ViewModels;

public class CatchPointsViewModel : BaseViewModel
{
    public ObservableCollection<CatchPointModel> FilteredPoints { get; private set; } = new();
    public bool IsFiltered => CurrentFish != null;
    public void RefreshFilteredPoints(FishModel? selectedFish)
    {
        if (selectedFish == null)
        {
            // если внутри VM есть запомненная рыба — используем её
            selectedFish = CurrentFish;
        }

        FilteredPoints.Clear();
        CurrentFish = selectedFish;

        // 🟢 Берём актуальные данные из DataStore
        var points = TFOHelperRedux.Services.DataStore.CatchPoints.AsEnumerable();
        var selectedMap = TFOHelperRedux.Services.DataStore.SelectedMap;   // ← объявление здесь
        var mode = TFOHelperRedux.Services.DataStore.CurrentMode;          // ← и текущий режим

        switch (mode)
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
                // при других режимах (например, Baits) ничего не фильтруем
                break;
        }

        foreach (var p in points)
            FilteredPoints.Add(p);
    }

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
    public CatchPointsViewModel()
    {
        foreach (var point in DataStore.CatchPoints)
        {
            point.MapName = DataStore.Maps.FirstOrDefault(m => m.ID == point.MapID)?.Name ?? "—";
            point.FishNames = string.Join(", ",
                point.FishIDs?.Select(id => DataStore.Fishes.FirstOrDefault(f => f.ID == id)?.Name)
                ?? new[] { "—" });
            CatchPoints.Add(point);
        }
        EditCatchPointWindowCommand = new RelayCommand(OpenEditCatchPointWindow);
        ImportPointsCmd = new RelayCommand(ImportPoints);
        ExportPointsCmd = new RelayCommand(ExportPoints);
        ClearPointsCmd = new RelayCommand(ClearPoints);
        SavePointsCmd = new RelayCommand(SavePoints);
        DeletePointCmd = new RelayCommand(p => DeletePoint(p as CatchPointModel));
        EditPointCmd = new RelayCommand(p => EditPoint(p as CatchPointModel));
    }
    private void DeletePoint(CatchPointModel? point)
    {
        if (point == null)
            return;

        var result = MessageBox.Show(
            $"Удалить точку лова на {point.MapName} (X={point.Coords.X}; Y={point.Coords.Y})?",
            "Удаление точки лова",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        // Удаляем из источника
        DataStore.CatchPoints.Remove(point);

        // Обновляем локальные коллекции и фильтр сразу, чтобы UI обновился мгновенно
        RefreshCatchPoints();
        RefreshFilteredPoints(CurrentFish);

        // Сохраняем изменения
        DataStore.SaveAll(); // сохранение + обновление фильтра в других местах
    }

    private void EditPoint(CatchPointModel? point)
    {
        if (point == null)
            return;

        var wnd = new TFOHelperRedux.Views.EditCatchPointWindow(point);
        if (wnd.ShowDialog() == true)
        {
            // после окна данные уже сохранены через SaveAll()
            var fish = TFOHelperRedux.Services.DataStore.SelectedFish ?? CurrentFish;
            RefreshFilteredPoints(fish);
        }
    }
    private void OpenEditCatchPointWindow()
    {
        var wnd = new TFOHelperRedux.Views.EditCatchPointWindow(); // 🪟 используем уже готовое окно
        if (wnd.ShowDialog() == true)
        {
            // После закрытия окна обновим список точек
            RefreshFilteredPoints(TFOHelperRedux.Services.DataStore.SelectedFish);
        }
    }
    private void ImportPoints()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            Title = "Импорт точек лова"
        };

        if (dlg.ShowDialog() == true)
        {
            DataStore.ImportCatchPoints(dlg.FileName);
            RefreshCatchPoints();
            MessageBox.Show("Импорт завершён ✅", "Импорт", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ExportPoints()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            Title = "Экспорт точек лова"
        };

        if (dlg.ShowDialog() == true)
        {
            DataStore.ExportCatchPoints(dlg.FileName);
            MessageBox.Show("Точки экспортированы 💾", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ClearPoints()
    {
        if (MessageBox.Show("Очистить все точки лова?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            DataStore.ClearCatchPoints();
            CatchPoints.Clear();
            MessageBox.Show("Точки очищены 🗑", "Очистка", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void SavePoints()
    {
        DataStore.SaveAll();
        MessageBox.Show("Изменения сохранены 💾", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void RefreshCatchPoints()
    {
        CatchPoints.Clear();
        foreach (var point in DataStore.CatchPoints)
        {
            point.MapName = DataStore.Maps.FirstOrDefault(m => m.ID == point.MapID)?.Name ?? "—";
            point.FishNames = string.Join(", ",
                point.FishIDs?.Select(id => DataStore.Fishes.FirstOrDefault(f => f.ID == id)?.Name)
                ?? new[] { "—" });
            CatchPoints.Add(point);
        }
    }
}
