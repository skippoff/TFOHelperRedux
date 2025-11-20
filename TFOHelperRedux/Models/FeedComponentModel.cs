using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TFOHelperRedux.Models
{
    public class FeedComponentModel : IItemModel, INotifyPropertyChanged
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public DateTime DateEdited { get; set; } = DateTime.Now;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}