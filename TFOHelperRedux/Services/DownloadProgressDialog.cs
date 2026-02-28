using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TFOHelperRedux.Services
{
    /// <summary>
    /// Диалоговое окно с прогресс-баром загрузки
    /// </summary>
    public class DownloadProgressDialog : Window
    {
        private readonly TextBlock _statusText;
        private readonly ProgressBar _progressBar;
        private readonly TextBlock _progressText;

        public DownloadProgressDialog()
        {
            Title = "Загрузка обновления";
            Width = 400;
            Height = 180;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;

            var mainGrid = new Grid { Margin = new Thickness(15) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Текст состояния
            _statusText = new TextBlock
            {
                Text = "Загрузка обновления...",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(_statusText, 0);

            // Прогресс-бар
            _progressBar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Height = 25,
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(_progressBar, 1);

            // Текст прогресса
            _progressText = new TextBlock
            {
                Text = "0%",
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 5, 0, 0)
            };
            Grid.SetRow(_progressText, 2);

            mainGrid.Children.Add(_statusText);
            mainGrid.Children.Add(_progressBar);
            mainGrid.Children.Add(_progressText);

            Content = mainGrid;
        }

        public void UpdateProgress(int percentage, long bytesLoaded, long totalBytes)
        {
            Dispatcher.Invoke(() =>
            {
                _progressBar.Value = percentage;
                _progressText.Text = $"{percentage}% ({FormatSize(bytesLoaded)} / {FormatSize(totalBytes)})";
            });
        }

        private static string FormatSize(long bytes)
        {
            string[] sizes = { "Б", "КБ", "МБ", "ГБ" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
