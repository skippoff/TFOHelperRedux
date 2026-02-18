using TFOHelperRedux.Services.Data;

namespace TFOHelperRedux.Models;
public class MapModel
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double MinDepth { get; set; }
    public double MaxDepth { get; set; }
    public int[] FishIDs { get; set; } = System.Array.Empty<int>();
    public bool DLC { get; set; }
    // 🔽 Новое: рабочая область PNG в пикселях (опционально)
    public int PixelLeft { get; set; }    // x левого края «квадрата карты»
    public int PixelTop { get; set; }     // y верхнего края
    public int PixelRight { get; set; }   // x правого края
    public int PixelBottom { get; set; }  // y нижнего края
    public string ImagePath => DataService.GetMapImagePath(ID);

}
