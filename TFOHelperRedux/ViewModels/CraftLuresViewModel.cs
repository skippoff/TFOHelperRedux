using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TFOHelperRedux.Helpers;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;

namespace TFOHelperRedux.ViewModels
{
    public class CraftLuresViewModel : BaseViewModel
    {
        public ObservableCollection<CraftLureModel> CraftLures { get; set; }
        private CraftLureModel? _current;

        public CraftLureModel? Current
        {
            get => _current;
            set
            {
                _current = value;
                Name = value?.Name ?? "";
                UpdatePreview();
                OnPropertyChanged();
            }
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        // Для визуализации состава (наживки + дипы)
        public ObservableCollection<string> PreviewItems { get; } = new();

        public ICommand SaveCmd { get; }
        public ICommand NewCmd { get; }
        public ICommand DeleteCmd { get; }
        public ICommand ClearCmd { get; }

        public ICommand AttachToFishCmd { get; }
        public ICommand DetachFromFishCmd { get; }

        public CraftLuresViewModel()
        {
            CraftLures = DataStore.CraftLures;

            SaveCmd = new RelayCommand(_ => Save());
            NewCmd = new RelayCommand(_ => New());
            DeleteCmd = new RelayCommand(_ => Delete());
            ClearCmd = new RelayCommand(_ => Clear());

            AttachToFishCmd = new RelayCommand(AttachToFish);
            DetachFromFishCmd = new RelayCommand(DetachFromFish);

            // Связь с панелью наживок: добавление в текущий рецепт по двойному клику
            DataStore.AddToCraftLure = AddToCurrent; // ЭТО поле нужно добавить в DataStore
        }

        public void AddToCurrent(IItemModel item)
        {
            if (item == null) return;
            if (Current == null)
                Current = new CraftLureModel { Name = "Новая крафтовая наживка" };

            switch (item)
            {
                case LureModel lure:
                    if (!Current.LureIDs.Contains(lure.ID))
                        Current.LureIDs = Current.LureIDs.Append(lure.ID).ToArray();
                    break;

                case DipModel dip:
                    if (!Current.DipIDs.Contains(dip.ID))
                        Current.DipIDs = Current.DipIDs.Append(dip.ID).ToArray();
                    break;
            }

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            PreviewItems.Clear();
            if (Current == null) return;

            foreach (var id in Current.LureIDs)
                PreviewItems.Add($"Наживка: {DataStore.Lures.FirstOrDefault(l => l.ID == id)?.Name ?? id.ToString()}");

            foreach (var id in Current.DipIDs)
                PreviewItems.Add($"Дип: {DataStore.Dips.FirstOrDefault(d => d.ID == id)?.Name ?? id.ToString()}");

            OnPropertyChanged(nameof(PreviewItems));
        }

        private void Save()
        {
            if (Current == null || string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Введите название крафтовой наживки.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Current.Name = Name;
            Current.DateEdited = DateTime.Now;

            if (Current.ID == 0)
            {
                // простой генератор ID (можно улучшить при необходимости)
                var maxId = CraftLures.Any() ? CraftLures.Max(c => c.ID) : 0;
                Current.ID = maxId + 1;
            }

            var existing = CraftLures.FirstOrDefault(c => c.ID == Current.ID);
            if (existing != null)
            {
                existing.Name = Current.Name;
                existing.LureIDs = Current.LureIDs;
                existing.DipIDs = Current.DipIDs;
                existing.FishIDs = Current.FishIDs;
                existing.Rank = Current.Rank;
                existing.DateEdited = Current.DateEdited;
            }
            else
            {
                CraftLures.Add(Current);
            }

            DataService.SaveCraftLures(CraftLures);
            MessageBox.Show("Крафтовая наживка сохранена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void New()
        {
            Current = new CraftLureModel { Name = "Новая крафтовая наживка" };
            Name = "";
            PreviewItems.Clear();
        }

        private void Clear()
        {
            if (Current == null) return;

            if (MessageBox.Show("Очистить состав крафтовой наживки?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Current.LureIDs = Array.Empty<int>();
                Current.DipIDs = Array.Empty<int>();
                UpdatePreview();
            }
        }

        private void Delete()
        {
            if (Current == null) return;

            if (MessageBox.Show($"Удалить крафтовую наживку '{Current.Name}'?", "Удаление",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                CraftLures.Remove(Current);
                DataService.SaveCraftLures(CraftLures);
                New();
            }
        }

        private void AttachToFish(object? parameter)
        {
            if (parameter is not CraftLureModel craft) return;
            var fish = DataStore.SelectedFish; // нужно завести такое поле в DataStore, или получать иначе

            if (fish == null)
            {
                MessageBox.Show("Сначала выберите рыбу.", "Привязка", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!craft.FishIDs.Contains(fish.ID))
                craft.FishIDs = craft.FishIDs.Append(fish.ID).ToArray();

            DataService.SaveCraftLures(CraftLures);
        }

        private void DetachFromFish(object? parameter)
        {
            if (parameter is not CraftLureModel craft) return;
            var fish = DataStore.SelectedFish;

            if (fish == null)
            {
                MessageBox.Show("Сначала выберите рыбу.", "Отвязка", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (craft.FishIDs.Contains(fish.ID))
                craft.FishIDs = craft.FishIDs.Where(id => id != fish.ID).ToArray();

            DataService.SaveCraftLures(CraftLures);
        }
    }
}
