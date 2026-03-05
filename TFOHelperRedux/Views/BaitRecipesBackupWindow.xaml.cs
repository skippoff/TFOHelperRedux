using System.Collections.Generic;
using System.Windows;
using TFOHelperRedux.Services.Business;

namespace TFOHelperRedux.Views;

public partial class BaitRecipesBackupWindow : Window
{
    public List<BackupFileInfo> Backups { get; }
    public BackupFileInfo? SelectedBackup { get; private set; }

    public BaitRecipesBackupWindow(List<BackupFileInfo> backups)
    {
        Backups = backups;
        InitializeComponent();
        DataContext = this;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void RestoreButton_Click(object sender, RoutedEventArgs e)
    {
        // Получаем выбранный элемент из ListBox
        SelectedBackup = BackupsListBox.SelectedItem as BackupFileInfo;
        
        if (SelectedBackup != null)
        {
            DialogResult = true;
            Close();
        }
    }
}
