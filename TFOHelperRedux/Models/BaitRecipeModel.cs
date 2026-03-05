using System;
using System.ComponentModel;

namespace TFOHelperRedux.Models
{
    public class BaitRecipeModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private RecipeRank _rank = RecipeRank.Normal;

        public int ID { get; set; }
        public string Name { get; set; } = "";
        public int[] FeedIDs { get; set; } = Array.Empty<int>();
        public int[] LureIDs { get; set; } = Array.Empty<int>();
        public int[] DipIDs { get; set; } = Array.Empty<int>();
        public int[] ComponentIDs { get; set; } = Array.Empty<int>();
        public int[] FishIDs { get; set; } = Array.Empty<int>();
        public DateTime DateEdited { get; set; } = DateTime.Now;
        public bool IsHidden { get; set; } = false;

        public RecipeRank Rank
        {
            get => _rank;
            set
            {
                if (_rank != value)
                {
                    _rank = value;
                    OnPropertyChanged(nameof(Rank));
                }
            }
        }

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

        /// <summary>
        /// Публичный метод для уведомления об изменении свойства (для использования в ViewModel)
        /// </summary>
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Нормализует null значения свойств после загрузки из JSON
        /// </summary>
        public void Normalize()
        {
            Name ??= "";
            FeedIDs ??= Array.Empty<int>();
            LureIDs ??= Array.Empty<int>();
            DipIDs ??= Array.Empty<int>();
            ComponentIDs ??= Array.Empty<int>();
            FishIDs ??= Array.Empty<int>();
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