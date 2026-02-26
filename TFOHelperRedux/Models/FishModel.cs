using System;
using System.ComponentModel;
using System.IO;
using TFOHelperRedux.Services.Data;

namespace TFOHelperRedux.Models
{
    public class FishModel : ValidatableModel, IItemModel
    {
        private bool _isSelected;
        private int _id;
        private string _name = "";
        private string _comment = "";
        private string _imagePath = "";
        private int _weightLarge;
        private int _weightTrophy;
        private int _hookMin;
        private int _hookMax;
        private int _temperatureMin;
        private int _temperatureMax;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

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

        public int WeightLarge
        {
            get => _weightLarge;
            set => SetProperty(ref _weightLarge, value);
        }

        public int WeightTrophy
        {
            get => _weightTrophy;
            set => SetProperty(ref _weightTrophy, value);
        }

        public int HookMin
        {
            get => _hookMin;
            set => SetProperty(ref _hookMin, value);
        }

        public int HookMax
        {
            get => _hookMax;
            set => SetProperty(ref _hookMax, value);
        }

        public int TemperatureMin
        {
            get => _temperatureMin;
            set => SetProperty(ref _temperatureMin, value);
        }

        public int TemperatureMax
        {
            get => _temperatureMax;
            set => SetProperty(ref _temperatureMax, value);
        }

        private int[] _activeTimes = Array.Empty<int>();
        private int[] _biteIntensity = Enumerable.Repeat(0, 24).ToArray();

        public int[] ActiveTimes
        {
            get => _activeTimes;
            set => SetProperty(ref _activeTimes, value ?? Array.Empty<int>());
        }

        public int[] BiteIntensity
        {
            get => _biteIntensity;
            set => SetProperty(ref _biteIntensity, value ?? Enumerable.Repeat(0, 24).ToArray());
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

            if (propertyName == null || propertyName == nameof(HookMin))
            {
                if (HookMin < 0)
                    AddError(nameof(HookMin), "Мин. размер крючка должен быть ≥ 0");
                else
                    RemoveError(nameof(HookMin));
            }

            if (propertyName == null || propertyName == nameof(HookMax))
            {
                if (HookMax < 0)
                    AddError(nameof(HookMax), "Макс. размер крючка должен быть ≥ 0");
                else if (HookMax < HookMin)
                    AddError(nameof(HookMax), "Макс. размер должен быть ≥ мин.");
                else
                    RemoveError(nameof(HookMax));
            }

            if (propertyName == null || propertyName == nameof(TemperatureMin))
            {
                if (TemperatureMin < -50 || TemperatureMin > 50)
                    AddError(nameof(TemperatureMin), "Температура должна быть от -50 до 50");
                else
                    RemoveError(nameof(TemperatureMin));
            }

            if (propertyName == null || propertyName == nameof(TemperatureMax))
            {
                if (TemperatureMax < -50 || TemperatureMax > 50)
                    AddError(nameof(TemperatureMax), "Температура должна быть от -50 до 50");
                else if (TemperatureMax < TemperatureMin)
                    AddError(nameof(TemperatureMax), "Макс. температура должна быть ≥ мин.");
                else
                    RemoveError(nameof(TemperatureMax));
            }

            if (propertyName == null || propertyName == nameof(WeightLarge))
            {
                if (WeightLarge < 0)
                    AddError(nameof(WeightLarge), "Вес должен быть ≥ 0");
                else
                    RemoveError(nameof(WeightLarge));
            }

            if (propertyName == null || propertyName == nameof(WeightTrophy))
            {
                if (WeightTrophy < 0)
                    AddError(nameof(WeightTrophy), "Вес должен быть ≥ 0");
                else
                    RemoveError(nameof(WeightTrophy));
            }
        }
    }
}
