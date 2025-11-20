using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TFOHelperRedux.Models;

namespace TFOHelperRedux.Views
{
    public partial class MapPreviewWindow : Window
    {
        private MapModel? _map;
        private CatchPointModel? _point;
#if DEBUG
        // 🔧 калибровка границ внутреннего квадрата карты
        private bool _calibrating = false;
        private System.Windows.Point? _calibBL; // нижний левый (0,0)
        private System.Windows.Point? _calibTR; // верхний правый (Width, Height)
#endif
        public MapPreviewWindow(MapModel map, CatchPointModel point)
        {
            InitializeComponent();
            UpdatePoint(map, point);
            // центр экрана
            var sw = SystemParameters.PrimaryScreenWidth;
            var sh = SystemParameters.PrimaryScreenHeight;

            // базовый центр
            var centerLeft = (sw - Width) / 2;
            var centerTop = (sh - Height) / 2;

            // сдвигаем на 200 пикселей левее центра
            Left = centerLeft - 400;
            Top = centerTop;
#if DEBUG
            // в дев-сборке показываем пункт меню калибровки
            miCalibrate.Visibility = Visibility.Visible;
            // 🎯 включение режима калибровки клавишей F2
            this.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.F2)
                {
                    _calibrating = !_calibrating;
                    _calibBL = _calibTR = null;
                    Title = _calibrating
                        ? "Карта водоёма — КАЛИБРОВКА (кликни нижний левый, затем верхний правый)"
                        : "Карта водоёма";
                }
            };

            // 🖱️ выбор области кликами по карте
            Overlay.MouseLeftButtonDown += (s, e) =>
            {
                if (!_calibrating) return;

                var p = e.GetPosition(Overlay); // пиксели исходной картинки (через Viewbox)
                if (_calibBL == null)
                {
                    _calibBL = p;
                    Title = "Выбери верхний правый угол…";
                }
                else
                {
                    _calibTR = p;

                    // записываем границы
                    _map.PixelLeft = (int)Math.Round(Math.Min(_calibBL.Value.X, _calibTR.Value.X));
                    _map.PixelRight = (int)Math.Round(Math.Max(_calibBL.Value.X, _calibTR.Value.X));
                    _map.PixelBottom = (int)Math.Round(Math.Max(_calibBL.Value.Y, _calibTR.Value.Y));
                    _map.PixelTop = (int)Math.Round(Math.Min(_calibBL.Value.Y, _calibTR.Value.Y));

                    // сохраняем в JSON
                    var all = TFOHelperRedux.Services.DataStore.Maps;
                    TFOHelperRedux.Services.DataService.SaveMaps(all);

                    _calibrating = false;
                    Title = "Карта водоёма — калибровка сохранена";

                    // пересчёт позиции точки после сохранения
                    RepositionMarker();
                }
            };
#endif
        }
        public void UpdatePoint(MapModel map, CatchPointModel point)
        {
            _map = map;
            _point = point;

            string path = map.ImagePath;
            if (!File.Exists(path))
            {
                MessageBox.Show($"Карта не найдена: {path}");
                return;
            }

            var bmp = new BitmapImage(new Uri(path, UriKind.Absolute));
            MapImage.Source = bmp;

            Overlay.Width = bmp.PixelWidth;
            Overlay.Height = bmp.PixelHeight;

            RepositionMarker();
            DataContext = this;
        }

        // 📍 Расчёт позиции точки с учётом ROI (рабочей области)
        private void RepositionMarker()
        {
            if (_map == null || _point == null || MapImage.Source == null)
                return;

            if (MapImage.Source is not BitmapSource bmp)
                return;

            double imgW = bmp.PixelWidth;
            double imgH = bmp.PixelHeight;

            // 🔹 Границы рабочей области PNG
            int L = _map.PixelLeft > 0 ? _map.PixelLeft : 0;
            int T = _map.PixelTop > 0 ? _map.PixelTop : 0;
            int R = _map.PixelRight > 0 ? _map.PixelRight : (int)imgW;
            int B = _map.PixelBottom > 0 ? _map.PixelBottom : (int)imgH;

            double roiW = Math.Max(0, R - L);
            double roiH = Math.Max(0, B - T);
            if (roiW <= 0 || roiH <= 0)
            {
                L = 0; T = 0; R = (int)imgW; B = (int)imgH;
                roiW = imgW; roiH = imgH;
            }

            // нормализуем игровые координаты
            double xRatio = _map.Width > 0 ? _point.Coords.X / (double)_map.Width : 0.0;
            double yRatio = _map.Height > 0 ? _point.Coords.Y / (double)_map.Height : 0.0;

            // перевод в пиксели внутри ROI + инверсия Y
            double pxImage = L + xRatio * roiW;
            double pyImage = B - yRatio * roiH;

            // позиционируем маркер
            Canvas.SetLeft(Marker, pxImage - Marker.Width / 2);
            Canvas.SetTop(Marker, pyImage - Marker.Height / 2);
        }
        public string CoordsInfo => $"Координаты: {_point?.Coords.X}:{_point?.Coords.Y}";
        public string FishName => _point != null && _point.FishIDs?.Length > 0
            ? $"Рыба: {TFOHelperRedux.Services.DataStore.Fishes.FirstOrDefault(f => f.ID == _point.FishIDs[0])?.Name ?? "Неизвестно"}"
            : "Рыба: —";
        // 🧩 "О программе"
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "TFOHelperRedux\n\nПрограмма для работы с картами водоёмов, точками лова и рыбой." +
                "Переиздание сделано на основе TFOHelper автор которой PilGrim." +
                "Сделано под свой вкус. \n\n© Skipoff, 2025",
                "О программе",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // 🧭 Режим калибровки карт
        private void CalibMode_Checked(object sender, RoutedEventArgs e)
        {
#if DEBUG
            _calibrating = true;
            _calibBL = _calibTR = null;
            Title = "Карта водоёма — КАЛИБРОВКА (кликни нижний левый, затем верхний правый)";
#endif
        }

        private void CalibMode_Unchecked(object sender, RoutedEventArgs e)
        {
#if DEBUG
            _calibrating = false;
            Title = "Карта водоёма";
#endif
        }

    }
}
