using System;
using System.ComponentModel;

namespace TFOHelperRedux.Models
{
    public class FeedComponentModel : ValidatableModel, IItemModel
    {
        private int _id;
        private string _name = string.Empty;
        private string _comment = string.Empty;
        private string _imagePath = string.Empty;
        private DateTime _dateEdited = DateTime.Now;
        private bool _isSelected;

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

        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value ?? string.Empty);
        }

        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value ?? string.Empty);
        }

        public DateTime DateEdited
        {
            get => _dateEdited;
            set => SetProperty(ref _dateEdited, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
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

            if (propertyName == null || propertyName == nameof(DateEdited))
            {
                if (DateEdited > DateTime.Now)
                    AddError(nameof(DateEdited), "Дата редактирования не может быть в будущем");
                else
                    RemoveError(nameof(DateEdited));
            }
        }
    }
}