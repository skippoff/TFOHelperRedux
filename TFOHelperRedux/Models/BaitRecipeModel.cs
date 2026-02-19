using System;
using System.ComponentModel;

namespace TFOHelperRedux.Models
{
    public class BaitRecipeModel : INotifyPropertyChanged
    {
        private bool _isSelected;

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

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public enum RecipeRank
    {
        Normal,       // Синий
        Top,          // Зеленый
        Deprecated,   // Красный
        Experimental  // Оранжевый
    }
}