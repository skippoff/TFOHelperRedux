using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using TFOHelperRedux.Services.Data;

namespace TFOHelperRedux.Models;

public interface IItemModel
{
    int ID { get; set; }
    string Name { get; set; }
    string Comment { get; set; }
    string ImagePath { get; set; }
}

public class BaitModel : ValidatableModel, IItemModel
{
    private static readonly Dictionary<int, string> _imagePathCache = new();
    
    private bool _isSelected;
    private int _id;
    private string _name = string.Empty;
    private string _imagePath = string.Empty;
    private string _comment = string.Empty;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public int ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value ?? string.Empty);
    }

    public string ImagePath
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_imagePath) && File.Exists(_imagePath))
                return _imagePath;

            // Проверяем кэш
            if (_imagePathCache.TryGetValue(ID, out var cachedPath))
                return cachedPath;

            // Загружаем и кэшируем
            var p = DataService.GetFeedImagePath(ID);
            if (File.Exists(p))
            {
                _imagePathCache[ID] = p;
                return p;
            }

            _imagePathCache[ID] = string.Empty;
            return string.Empty;
        }
        set => SetProperty(ref _imagePath, value ?? string.Empty);
    }

    public string Comment
    {
        get => _comment;
        set => SetProperty(ref _comment, value ?? string.Empty);
    }

    public int[] ComponentIDs { get; set; } = Array.Empty<int>();

    protected override void Validate(string? propertyName = null)
    {
        base.Validate(propertyName);

        if (propertyName == null || propertyName == nameof(ID))
        {
            if (ID < 0)
                AddError(nameof(ID), "ID должен быть неотрицательным");
            else
                RemoveError(nameof(ID));
        }

        if (propertyName == null || propertyName == nameof(Name))
        {
            if (string.IsNullOrWhiteSpace(Name))
                AddError(nameof(Name), "Название обязательно");
            else if (Name.Length > 100)
                AddError(nameof(Name), "Название не должно превышать 100 символов");
            else
                RemoveError(nameof(Name));
        }
    }
}

public class DipModel : ValidatableModel, IItemModel
{
    private static readonly Dictionary<int, string> _imagePathCache = new();
    
    private bool _isSelected;
    private int _id;
    private string _name = string.Empty;
    private string _imagePath = string.Empty;
    private string _comment = string.Empty;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public int ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value ?? string.Empty);
    }

    public string ImagePath
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_imagePath) && File.Exists(_imagePath))
                return _imagePath;

            // Проверяем кэш
            if (_imagePathCache.TryGetValue(ID, out var cachedPath))
                return cachedPath;

            // Загружаем и кэшируем
            var p = DataService.GetDipImagePath(ID);
            if (File.Exists(p))
            {
                _imagePathCache[ID] = p;
                return p;
            }

            _imagePathCache[ID] = string.Empty;
            return string.Empty;
        }
        set => SetProperty(ref _imagePath, value ?? string.Empty);
    }

    public string Comment
    {
        get => _comment;
        set => SetProperty(ref _comment, value ?? string.Empty);
    }

    protected override void Validate(string? propertyName = null)
    {
        base.Validate(propertyName);

        if (propertyName == null || propertyName == nameof(ID))
        {
            if (ID < 0)
                AddError(nameof(ID), "ID должен быть неотрицательным");
            else
                RemoveError(nameof(ID));
        }

        if (propertyName == null || propertyName == nameof(Name))
        {
            if (string.IsNullOrWhiteSpace(Name))
                AddError(nameof(Name), "Название обязательно");
            else if (Name.Length > 100)
                AddError(nameof(Name), "Название не должно превышать 100 символов");
            else
                RemoveError(nameof(Name));
        }
    }
}

public class LureModel : ValidatableModel, IItemModel
{
    private static readonly Dictionary<int, string> _imagePathCache = new();
    
    private bool _isSelected;
    private bool _isBestSelected;
    private int _id;
    private string _name = string.Empty;
    private string _imagePath = string.Empty;
    private string _comment = string.Empty;
    private string _baitType = "live";

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool IsBestSelected
    {
        get => _isBestSelected;
        set => SetProperty(ref _isBestSelected, value);
    }

    public int ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value ?? string.Empty);
    }

    public string BaitType
    {
        get => _baitType;
        set => SetProperty(ref _baitType, value ?? "live");
    }

    public string ImagePath
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_imagePath) && File.Exists(_imagePath))
                return _imagePath;

            // Проверяем кэш
            if (_imagePathCache.TryGetValue(ID, out var cachedPath))
                return cachedPath;

            // Загружаем и кэшируем
            var p = DataService.GetLureImagePath(ID);
            if (File.Exists(p))
            {
                _imagePathCache[ID] = p;
                return p;
            }

            _imagePathCache[ID] = string.Empty;
            return string.Empty;
        }
        set => SetProperty(ref _imagePath, value ?? string.Empty);
    }

    public string Comment
    {
        get => _comment;
        set => SetProperty(ref _comment, value ?? string.Empty);
    }

    protected override void Validate(string? propertyName = null)
    {
        base.Validate(propertyName);

        if (propertyName == null || propertyName == nameof(ID))
        {
            if (ID < 0)
                AddError(nameof(ID), "ID должен быть неотрицательным");
            else
                RemoveError(nameof(ID));
        }

        if (propertyName == null || propertyName == nameof(Name))
        {
            if (string.IsNullOrWhiteSpace(Name))
                AddError(nameof(Name), "Название обязательно");
            else if (Name.Length > 100)
                AddError(nameof(Name), "Название не должно превышать 100 символов");
            else
                RemoveError(nameof(Name));
        }

        if (propertyName == null || propertyName == nameof(BaitType))
        {
            if (BaitType != "live" && BaitType != "lure")
                AddError(nameof(BaitType), "Тип наживки должен быть 'live' или 'lure'");
            else
                RemoveError(nameof(BaitType));
        }
    }
}
