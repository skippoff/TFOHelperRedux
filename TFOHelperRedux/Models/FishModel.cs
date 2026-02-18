using System.ComponentModel;

namespace TFOHelperRedux.Models
{
    public class FishModel : INotifyPropertyChanged
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
        public string Name { get; set; } = "";
        public string Comment { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public int WeightLarge { get; set; }
        public int WeightTrophy { get; set; }
        public int HookMin { get; set; }
        public int HookMax { get; set; }
        public int TemperatureMin { get; set; }
        public int TemperatureMax { get; set; }
        public int[] FeedIDs { get; set; } = Array.Empty<int>();
        public int[] DipIDs { get; set; } = Array.Empty<int>();
        public int[] LureIDs { get; set; } = Array.Empty<int>();
        public int[] ActiveTimes { get; set; } = Array.Empty<int>();
        public int[] RecipeIDs { get; set; } = Array.Empty<int>();
        // 🔹 Лучшие (топовые) магазинные наживки
        public int[] BestLureIDs { get; set; } = Array.Empty<int>();
        // 🔹 Лучшие крафтовые наживки (по рецептам)
        public int[] BestRecipeIDs { get; set; } = Array.Empty<int>();
        //интенсивность клёва
        public int[] BiteIntensity { get; set; } = Enumerable.Repeat(0, 24).ToArray();


        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
