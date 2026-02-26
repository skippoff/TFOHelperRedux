using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace TFOHelperRedux.Models;

public class CatchPointModel : INotifyPropertyChanged
{
    private int _mapId;
    private Coords _coords = new();
    private int[] _fishIds = Array.Empty<int>();
    private int[] _lureIds = Array.Empty<int>();
    private int[] _feedIds = Array.Empty<int>();
    private int[] _bestLureIds = Array.Empty<int>();
    private int[] _dipsIds = Array.Empty<int>();
    private int[] _recipeIds = Array.Empty<int>();
    private int[] _times = Array.Empty<int>();
    private int[] _rods = Array.Empty<int>();
    private bool _cautious;
    private bool _trophy;
    private bool _tournament;
    private double _depthValue;
    private double _clipValue;
    private string _comment = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Триггерим зависимые свойства
        if (propertyName is nameof(Trophy) or nameof(Tournament) or nameof(Cautious))
        {
            OnPropertyChanged(nameof(FeaturesInfo));
        }
        if (propertyName is nameof(Times))
        {
            OnPropertyChanged(nameof(TimeInfo));
        }
        if (propertyName is nameof(Rods))
        {
            OnPropertyChanged(nameof(RodTypeInfo));
        }
        if (propertyName is nameof(DepthValue))
        {
            OnPropertyChanged(nameof(DepthInfo));
        }
        if (propertyName is nameof(ClipValue))
        {
            OnPropertyChanged(nameof(ClipInfo));
        }
    }

    public int MapID
    {
        get => _mapId;
        set { _mapId = value; OnPropertyChanged(); }
    }

    public Coords Coords
    {
        get => _coords;
        set { _coords = value; OnPropertyChanged(); }
    }

    public int[] FishIDs
    {
        get => _fishIds;
        set { _fishIds = value; OnPropertyChanged(); }
    }

    public int[] LureIDs
    {
        get => _lureIds ?? Array.Empty<int>();
        set { _lureIds = value; OnPropertyChanged(); }
    }

    public int[] FeedIDs
    {
        get => _feedIds ?? Array.Empty<int>();
        set { _feedIds = value; OnPropertyChanged(); }
    }

    public int[] BestLureIDs
    {
        get => _bestLureIds ?? Array.Empty<int>();
        set { _bestLureIds = value; OnPropertyChanged(); }
    }

    public int[] DipsIDs
    {
        get => _dipsIds ?? Array.Empty<int>();
        set { _dipsIds = value; OnPropertyChanged(); }
    }

    public int[] RecipeIDs
    {
        get => _recipeIds ?? Array.Empty<int>();
        set { _recipeIds = value; OnPropertyChanged(); }
    }

    public int[] Times
    {
        get => _times;
        set { _times = value; OnPropertyChanged(); }
    }

    public int[] Rods
    {
        get => _rods;
        set { _rods = value; OnPropertyChanged(); }
    }

    public bool Cautious
    {
        get => _cautious;
        set { _cautious = value; OnPropertyChanged(); }
    }

    public bool Trophy
    {
        get => _trophy;
        set { _trophy = value; OnPropertyChanged(); }
    }

    public bool Tournament
    {
        get => _tournament;
        set { _tournament = value; OnPropertyChanged(); }
    }

    public double DepthValue
    {
        get => _depthValue;
        set { _depthValue = value; OnPropertyChanged(); }
    }

    public double ClipValue
    {
        get => _clipValue;
        set { _clipValue = value; OnPropertyChanged(); }
    }

    public double Temperature { get; set; }
    public ThrowMarker? ThrowMarker { get; set; }

    public string Comment
    {
        get => _comment;
        set { _comment = value ?? string.Empty; OnPropertyChanged(); }
    }

    public string MadeBy { get; set; } = Environment.UserName;
    public DateTime DateEdited { get; set; } = DateTime.Now;

    [JsonIgnore]
    public string MapName { get; set; } = string.Empty;

    [JsonIgnore]
    public string FishNames { get; set; } = string.Empty;

    // 🔹 Особенности (трофейная, турнирная, осторожная)
    [JsonIgnore]
    public string FeaturesInfo
    {
        get
        {
            var parts = new List<string>();
            if (Trophy) parts.Add("Трофейная");
            if (Tournament) parts.Add("Турнирная");
            if (Cautious) parts.Add("Осторожная");

            return parts.Count == 0
                ? "Особенности: —"
                : "Особенности: " + string.Join(", ", parts);
        }
    }

    // 🔹 Время лова (утро / день / вечер / ночь)
    [JsonIgnore]
    public string TimeInfo
    {
        get
        {
            if (Times == null || Times.Length == 0)
                return "Время лова: —";

            var parts = new List<string>();
            foreach (var t in Times)
            {
                switch (t)
                {
                    case 1: parts.Add("Утро"); break;
                    case 2: parts.Add("День"); break;
                    case 3: parts.Add("Вечер"); break;
                    case 4: parts.Add("Ночь"); break;
                }
            }

            return parts.Count == 0
                ? "Время лова: —"
                : "Время лова: " + string.Join(", ", parts);
        }
    }

    // 🔹 Тип удилища (спиннинг / фидер / поплавок / нахлыст / морское)
    [JsonIgnore]
    public string RodTypeInfo
    {
        get
        {
            if (Rods == null || Rods.Length == 0)
                return "Тип удилища: —";

            var parts = new List<string>();
            foreach (var r in Rods)
            {
                switch (r)
                {
                    case 1: parts.Add("Спиннинг"); break;
                    case 2: parts.Add("Фидер"); break;
                    case 3: parts.Add("Поплавок"); break;
                    case 4: parts.Add("Нахлыст"); break;
                    case 5: parts.Add("Морское"); break;
                }
            }

            return parts.Count == 0
                ? "Тип удилища: —"
                : "Тип удилища: " + string.Join(", ", parts);
        }
    }

    // 🔹 Глубина
    [JsonIgnore]
    public string DepthInfo
    {
        get
        {
            if (DepthValue <= 0)
                return "Глубина: —";

            return $"Глубина: {DepthValue:0.##}";
        }
    }
    // 🔹 Клипса
    [JsonIgnore]
    public string ClipInfo
    {
        get
        {
            if (ClipValue <= 0)
                return "Клипса: —";

            return $"Клипса: {ClipValue:0.##}";
        }
    }
}

public class Coords
{
    public bool IsEmpty { get; set; } = false;
    public int X { get; set; }
    public int Y { get; set; }
}

public class ThrowMarker
{
    public Coords Coords { get; set; } = new();
    public int Azimuth { get; set; }
    public int Distance { get; set; }
}
