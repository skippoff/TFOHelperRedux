using System;

namespace TFOHelperRedux.Models
{
    public class BaitRecipeModel
    {
        public int ID { get; set; }
        public string Name { get; set; } = "";
        public int[] FeedIDs { get; set; } = Array.Empty<int>();
        public int[] LureIDs { get; set; } = Array.Empty<int>();
        public int[] DipIDs { get; set; } = Array.Empty<int>();
        public int[] ComponentIDs { get; set; } = Array.Empty<int>();
        public int[] FishIDs { get; set; } = Array.Empty<int>();
        public RecipeRank Rank { get; set; } = RecipeRank.Normal;
        public DateTime DateEdited { get; set; } = DateTime.Now;
        public bool IsHidden { get; set; } = false;
    }
    public enum RecipeRank
    {
        Normal,       // Синий
        Top,          // Зеленый
        Deprecated,   // Красный
        Experimental  // Оранжевый
    }
}