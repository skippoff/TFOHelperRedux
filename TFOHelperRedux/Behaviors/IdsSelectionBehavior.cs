using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Behaviors
{
    public static class IdsSelectionBehavior
    {
        // Источник: массив ID (напр. SelectedFish.FeedIDs / DipIDs / Tags)
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached(
                "Source",
                typeof(int[]),
                typeof(IdsSelectionBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSourceChanged));

        public static void SetSource(DependencyObject obj, int[]? value) => obj.SetValue(SourceProperty, value);
        public static int[]? GetSource(DependencyObject obj) => (int[]?)obj.GetValue(SourceProperty);

        // Текущий элемент ID (тот, что чекбокс представляет)
        public static readonly DependencyProperty ItemIdProperty =
            DependencyProperty.RegisterAttached(
                "ItemId",
                typeof(int),
                typeof(IdsSelectionBehavior),
                new PropertyMetadata(0));

        public static void SetItemId(DependencyObject obj, int value) => obj.SetValue(ItemIdProperty, value);
        public static int GetItemId(DependencyObject obj) => (int)obj.GetValue(ItemIdProperty);

        // Привязка к SelectedFish (чтобы можно было дернуть OnPropertyChanged у модели)
        public static readonly DependencyProperty FishProperty =
            DependencyProperty.RegisterAttached(
                "Fish",
                typeof(FishModel),
                typeof(IdsSelectionBehavior),
                new PropertyMetadata(null));

        public static void SetFish(DependencyObject obj, FishModel? value) => obj.SetValue(FishProperty, value);
        public static FishModel? GetFish(DependencyObject obj) => (FishModel?)obj.GetValue(FishProperty);

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CheckBox cb)
            {
                cb.Checked -= OnChecked;
                cb.Unchecked -= OnUnchecked;
                cb.Loaded -= OnLoaded;

                cb.Checked += OnChecked;
                cb.Unchecked += OnUnchecked;
                cb.Loaded += OnLoaded;
            }
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb)
            {
                var ids = GetSource(cb) ?? Array.Empty<int>();
                var id = GetItemId(cb);
                cb.IsChecked = ids.Contains(id);
            }
        }

        private static void OnChecked(object sender, RoutedEventArgs e) => Update(sender as CheckBox, add: true);
        private static void OnUnchecked(object sender, RoutedEventArgs e) => Update(sender as CheckBox, add: false);

        private static void Update(CheckBox? cb, bool add)
        {
            if (cb == null) return;

            var ids = GetSource(cb) ?? Array.Empty<int>();
            var id = GetItemId(cb);
            var fish = GetFish(cb);

            int[] next;
            if (add)
            {
                if (ids.Contains(id)) return;
                next = ids.Concat(new[] { id }).Distinct().ToArray();
            }
            else
            {
                if (!ids.Contains(id)) return;
                next = ids.Where(x => x != id).ToArray();
            }

            // Переприсваиваем массив обратно в нужное свойство у FishModel
            // Определяем, каким именно массивом управляет данный чекбокс по ReferenceEquals
            if (fish != null)
            {
                if (ReferenceEquals(ids, fish.FeedIDs)) fish.FeedIDs = next;
                else if (ReferenceEquals(ids, fish.DipIDs)) fish.DipIDs = next;
                else if (ReferenceEquals(ids, fish.LureIDs)) fish.LureIDs = next;
                else if (ReferenceEquals(ids, fish.Tags)) fish.Tags = next;
                else fish.Tags = next; // дефолт на всякий
            }

            // Обновим Source DP у чекбокса, чтобы IsChecked пересчитался
            SetSource(cb, next);
        }
    }
}
