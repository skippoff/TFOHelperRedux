using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;

namespace TFOHelperRedux.Views
{
    public partial class AddFishToMapWindow : Window
    {
        public FishModel SelectedFish { get; private set; }
        // Коллекции для списков
        public ObservableCollection<IdName> Categories { get; set; } = new();
        public ObservableCollection<BaitModel> Feeds { get; set; }
        public ObservableCollection<DipModel> Dips { get; set; }
        public ObservableCollection<LureModel> Lures { get; set; }
        public class IdName
        {
            public int ID { get; set; }
            public string Name { get; set; } = "";
            public bool IsSelected { get; set; }
        }

        public AddFishToMapWindow(FishModel fish)
        {
            InitializeComponent();
            SelectedFish = fish ?? new FishModel();
            // гарантируем, что массив есть
            if (SelectedFish.ActiveTimes == null)
                SelectedFish.ActiveTimes = Array.Empty<int>();
            // Загружаем справочники один раз
            Feeds = DataStore.Feeds;
            Dips = DataStore.Dips;
            Lures = DataStore.Lures;
            // Категории из TagModel или фиксированных 4 штуки
            Categories = new ObservableCollection<IdName>();
            BuildCategories();
            // Привязка данных
            DataContext = this;
            // Загрузка данных в элементы
            LoadFishData();
        }
        private void BuildCategories()
        {
            Categories.Clear();

            if (DataStore.Tags != null && DataStore.Tags.Any())
            {
                foreach (var tag in DataStore.Tags)
                    Categories.Add(new IdName { ID = tag.ID, Name = tag.Name });
            }
            else
            {
                Categories.Add(new IdName { ID = 1, Name = "Карповые" });
                Categories.Add(new IdName { ID = 2, Name = "Лососевые" });
                Categories.Add(new IdName { ID = 3, Name = "Окунёвые" });
                Categories.Add(new IdName { ID = 4, Name = "Морская" });
            }
        }

        private void LoadFishData()
        {
            if (SelectedFish == null)
                return;

            // 🐟 Основные данные
            txtName.Text = SelectedFish.Name ?? "";
            txtLarge.Text = SelectedFish.WeightLarge.ToString();
            txtTrophy.Text = SelectedFish.WeightTrophy.ToString();
            txtHookMin.Text = SelectedFish.HookMin.ToString();
            txtHookMax.Text = SelectedFish.HookMax.ToString();
            txtTempMin.Text = SelectedFish.TemperatureMin.ToString();
            txtTempMax.Text = SelectedFish.TemperatureMax.ToString();
            txtComment.Text = SelectedFish.Comment ?? "";

            // 🖼 Загрузка изображения
            if (!string.IsNullOrWhiteSpace(SelectedFish.ImagePath))
            {
                string path = SelectedFish.ImagePath;
                if (!Path.IsPathRooted(path))
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

                if (File.Exists(path))
                {
                    try
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.UriSource = new Uri(path, UriKind.Absolute);
                        bmp.EndInit();
                        bmp.Freeze();
                        imgPreview.Source = bmp;
                    }
                    catch { imgPreview.Source = null; }
                }
            }
        }

        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Выбор изображения рыбы",
                Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var bmp = new BitmapImage(new Uri(dlg.FileName));
                    imgPreview.Source = bmp;

                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string relPath = dlg.FileName.Replace(baseDir, "");
                    SelectedFish.ImagePath = relPath;
                }
                catch
                {
                    MessageBox.Show("Не удалось загрузить изображение.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFish == null)
                return;

            // 🐟 Обновляем поля
            SelectedFish.Name = txtName.Text.Trim();
            SelectedFish.Comment = txtComment.Text.Trim();

            int.TryParse(txtLarge.Text, out int wLarge);
            SelectedFish.WeightLarge = wLarge;

            int.TryParse(txtTrophy.Text, out int wTrophy);
            SelectedFish.WeightTrophy = wTrophy;

            int.TryParse(txtHookMin.Text, out int hookMin);
            SelectedFish.HookMin = hookMin;

            int.TryParse(txtHookMax.Text, out int hookMax);
            SelectedFish.HookMax = hookMax;

            int.TryParse(txtTempMin.Text, out int tMin);
            SelectedFish.TemperatureMin = tMin;
            
            int.TryParse(txtTempMax.Text, out int tMax);
            SelectedFish.TemperatureMax = tMax;
            // Надёжная фиксация состояний времени суток:
            var times = new List<int>();
            if (cbMorning.IsChecked == true) times.Add(1);
            if (cbDay.IsChecked == true) times.Add(2);
            if (cbEvening.IsChecked == true) times.Add(3);
            if (cbNight.IsChecked == true) times.Add(4);
            SelectedFish.ActiveTimes = times.ToArray();

            // Сохраняем выборы IsSelected в моделях (чекбоксы привязаны к IsSelected)
            DataService.SaveFeeds(DataStore.Feeds);
            DataService.SaveDips(DataStore.Dips);
            DataService.SaveLures(DataStore.Lures);

            var existingFish = DataStore.Fishes.FirstOrDefault(f => f.ID == SelectedFish.ID);
            if (existingFish != null)
            {
                var index = DataStore.Fishes.IndexOf(existingFish);
                DataStore.Fishes[index] = SelectedFish;
            }
            else
            {
                SelectedFish.ID = DataStore.Fishes.Any() ? DataStore.Fishes.Max(f => f.ID) + 1 : 1;
                DataStore.Fishes.Add(SelectedFish);
            }

            // 💾 Сохраняем все изменения
            DataService.SaveFishes(DataStore.Fishes); // ✅ сохранение рыб
            DataStore.SaveAll();                      // сохранение остальных данных

            DialogResult = true;
            Close();
        }

        // Removed manual checkbox scanning; models now expose IsSelected properties bound directly to checkboxes.

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void Category_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedFish == null) return;
            if (sender is CheckBox cb && cb.Tag is int id)
            {
                if (cb.IsChecked == true)
                    SelectedFish.Tags = (SelectedFish.Tags ?? Array.Empty<int>()).Concat(new[] { id }).Distinct().ToArray();
                else
                    SelectedFish.Tags = (SelectedFish.Tags ?? Array.Empty<int>()).Where(x => x != id).ToArray();
            }
        }

        private void Feed_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedFish == null) return;
            if (sender is CheckBox cb && cb.Tag is int id)
            {
                if (cb.IsChecked == true)
                    SelectedFish.FeedIDs = (SelectedFish.FeedIDs ?? Array.Empty<int>()).Concat(new[] { id }).Distinct().ToArray();
                else
                    SelectedFish.FeedIDs = (SelectedFish.FeedIDs ?? Array.Empty<int>()).Where(x => x != id).ToArray();
            }
        }

        private void Dip_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedFish == null) return;
            if (sender is CheckBox cb && cb.Tag is int id)
            {
                if (cb.IsChecked == true)
                    SelectedFish.DipIDs = (SelectedFish.DipIDs ?? Array.Empty<int>()).Concat(new[] { id }).Distinct().ToArray();
                else
                    SelectedFish.DipIDs = (SelectedFish.DipIDs ?? Array.Empty<int>()).Where(x => x != id).ToArray();
            }
        }

        private void Lure_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedFish == null) return;
            if (sender is CheckBox cb && cb.Tag is int id)
            {
                if (cb.IsChecked == true)
                    SelectedFish.LureIDs = (SelectedFish.LureIDs ?? Array.Empty<int>()).Concat(new[] { id }).Distinct().ToArray();
                else
                    SelectedFish.LureIDs = (SelectedFish.LureIDs ?? Array.Empty<int>()).Where(x => x != id).ToArray();
            }
        }

        private void TimeOfDay_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag != null &&
                int.TryParse(cb.Tag.ToString(), out int id))
            {
                var arr = SelectedFish?.ActiveTimes ?? Array.Empty<int>();
                cb.IsChecked = arr.Contains(id);
            }
        }

        private void Category_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is int id)
                cb.IsChecked = SelectedFish?.Tags?.Contains(id) == true;
        }

        private void Feed_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is int id)
                cb.IsChecked = SelectedFish?.FeedIDs?.Contains(id) == true;
        }

        private void Dip_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is int id)
                cb.IsChecked = SelectedFish?.DipIDs?.Contains(id) == true;
        }

        private void Lure_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is int id)
                cb.IsChecked = SelectedFish?.LureIDs?.Contains(id) == true;
        }
        private void TimeOfDay_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedFish == null) return;
            if (sender is CheckBox cb && cb.Tag != null &&
                int.TryParse(cb.Tag.ToString(), out int id))
            {
                var list = SelectedFish.ActiveTimes?.ToList() ?? new List<int>();

                if (cb.IsChecked == true)
                {
                    if (!list.Contains(id)) list.Add(id);
                }
                else
                {
                    list.Remove(id);
                }

                // Сохраняем отсортированным (красиво)
                SelectedFish.ActiveTimes = list.OrderBy(x => x).ToArray();
            }
        }
    }
}
