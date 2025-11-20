using System;

namespace TFOHelperRedux.Models
{
    public class CraftLureModel
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;

        // Базовые наживки, которые участвуют в рецепте
        public int[] LureIDs { get; set; } = Array.Empty<int>();

        // Дипы для этого крафтового варианта
        public int[] DipIDs { get; set; } = Array.Empty<int>();

        // Рыбы, для которых эта крафтовая наживка считается подходящей
        public int[] FishIDs { get; set; } = Array.Empty<int>();

        public RecipeRank Rank { get; set; } = RecipeRank.Normal;

        public DateTime DateEdited { get; set; }
    }
}