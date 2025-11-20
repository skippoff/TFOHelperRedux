using System.Text.Json.Serialization;

namespace TFOHelperRedux.Models;

public class CatchPointModel
{
    public int MapID { get; set; }
    public Coords Coords { get; set; } = new();
    public int[] FishIDs { get; set; } = Array.Empty<int>();
    public int[] LureIDs { get; set; } = Array.Empty<int>();
    public int[] FeedIDs { get; set; } = Array.Empty<int>();
    public int[] DipsIDs { get; set; } = Array.Empty<int>();
    public int[] Times { get; set; } = Array.Empty<int>();
    public int[] Rods { get; set; } = Array.Empty<int>();
    public bool Cautious { get; set; } = false;
    public bool Trophy { get; set; }
    public bool Tournament { get; set; }
    public double DepthValue { get; set; }
    public double ClipValue { get; set; }
    public double Temperature { get; set; }
    public ThrowMarker? ThrowMarker { get; set; }
    public string Comment { get; set; } = string.Empty;
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
