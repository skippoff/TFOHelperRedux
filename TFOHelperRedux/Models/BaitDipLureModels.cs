using System.ComponentModel;
using System.IO;
using TFOHelperRedux.Services;

namespace TFOHelperRedux.Models;

public interface IItemModel
{
    int ID { get; set; }
    string Name { get; set; }
    string Comment { get; set; }
    string ImagePath { get; set; }
}

public class BaitModel : IItemModel, INotifyPropertyChanged
{
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }

    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;

    // Холдит пользовательский путь, если задан
    private string _imagePath = string.Empty;
    public string ImagePath
    {
        get
        {
            // 1) если путь задан и файл существует – используем его (пользовательский путь)
            if (!string.IsNullOrWhiteSpace(_imagePath) && File.Exists(_imagePath))
                return _imagePath;

            // 2) иначе пробуем стандартный путь по ID из папки Feeds
            var p = DataService.GetFeedImagePath(ID);
            return File.Exists(p) ? p : string.Empty;
        }
        set
        {
            if (_imagePath != value)
            {
                _imagePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImagePath)));
            }
        }
    }
    private string _comment = string.Empty;
    public string Comment
    {
        get => _comment;
        set
        {
            if (_comment != value)
            {
                _comment = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Comment)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public int[] ComponentIDs { get; set; } = Array.Empty<int>();
}

public class DipModel : IItemModel, INotifyPropertyChanged
{
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }

    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;

    private string _imagePath = string.Empty;
    public string ImagePath
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_imagePath) && File.Exists(_imagePath))
                return _imagePath;

            var p = DataService.GetDipImagePath(ID);
            return File.Exists(p) ? p : string.Empty;
        }
        set
        {
            if (_imagePath != value)
            {
                _imagePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImagePath)));
            }
        }
    }

    private string _comment = string.Empty;
    public string Comment
    {
        get => _comment;
        set
        {
            if (_comment != value)
            {
                _comment = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Comment)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public class LureModel : IItemModel, INotifyPropertyChanged
{
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }

    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;

    // Тип наживки: "live" (живая) или "lure" (искусственная приманка)
    private string _baitType = "live";
    public string BaitType
    {
        get => _baitType;
        set
        {
            if (_baitType != value)
            {
                _baitType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BaitType)));
            }
        }
    }

    // Холдит пользовательский путь, если задан
    private string _imagePath = string.Empty;
    public string ImagePath
    {
        get
        {
            // 1) если путь задан и файл существует – используем его (пользовательский путь)
            if (!string.IsNullOrWhiteSpace(_imagePath) && File.Exists(_imagePath))
                return _imagePath;

            var p = DataService.GetLureImagePath(ID);
            return File.Exists(p) ? p : string.Empty;
        }
        set
        {
            if (_imagePath != value)
            {
                _imagePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImagePath)));
            }
        }
    }

    private string _comment = string.Empty;
    public string Comment
    {
        get => _comment;
        set
        {
            if (_comment != value)
            {
                _comment = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Comment)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
