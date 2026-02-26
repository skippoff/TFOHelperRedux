using System.Windows;
using TFOHelperRedux.Services;

namespace TFOHelperRedux.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _settingsService;

        public SettingsWindow(SettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;

            // Загрузка текущих настроек
            var settings = _settingsService.GetSettings();
            TxtNickName.Text = settings.NickName;
            ChkShowNickName.IsChecked = settings.ShowNickNameInExport;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var nickName = TxtNickName.Text.Trim();
            var showNickName = ChkShowNickName.IsChecked == true;

            // Сохранение настроек
            _settingsService.UpdateNickName(nickName);
            _settingsService.UpdateShowNickName(showNickName);

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
