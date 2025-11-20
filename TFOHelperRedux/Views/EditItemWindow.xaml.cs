using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;
using TFOHelperRedux.Models;
using TFOHelperRedux.Services;

namespace TFOHelperRedux.Views;

public partial class EditItemWindow : Window
{
    private readonly IItemModel _original;
    private readonly IItemModel _workingCopy;

    public EditItemWindow(IItemModel item)
    {
        InitializeComponent();
        _original = item ?? throw new ArgumentNullException(nameof(item));
        _workingCopy = CreateCopy(item);
        DataContext = _workingCopy;
    }

    // Копируем свойства в новый экземпляр того же типа (чтобы отмена не изменила оригинал)
    private IItemModel CreateCopy(IItemModel src)
    {
        var type = src.GetType();
        var copy = (IItemModel)Activator.CreateInstance(type)!;
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.CanRead && prop.CanWrite)
            {
                var val = prop.GetValue(src);
                prop.SetValue(copy, val);
            }
        }
        return copy;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        // Валидация: обязательно имя
        if (string.IsNullOrWhiteSpace(_workingCopy.Name))
        {
            MessageBox.Show("Заполните поле «Название».", "Валидация", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Обработка изображения: если указан абсолютный путь — копируем в папку приложения
            if (!string.IsNullOrWhiteSpace(_workingCopy.ImagePath))
            {
                var src = _workingCopy.ImagePath;
                string folderName;
                string targetDir;

                // Определяем целевую папку по типу модели
                if (_original is LureModel)
                {
                    folderName = "Lures";
                    targetDir = DataService.LuresDir;
                }
                else if (_original is BaitModel)
                {
                    folderName = "Feeds";
                    targetDir = DataService.FeedsDir;
                }
                else if (_original is DipModel)
                {
                    folderName = "Dips";
                    targetDir = DataService.DipsDir;
                }
                else if (_original is FeedComponentModel)
                {
                    folderName = "FeedComponents";
                    targetDir = DataService.FeedComponentsDir;
                }
                else
                {
                    folderName = string.Empty;
                    targetDir = DataService.BaseDir;
                }

                // Если путь абсолютный и файл существует — копируем
                if (Path.IsPathRooted(src) && File.Exists(src))
                {
                    var ext = Path.GetExtension(src);
                    if (string.IsNullOrEmpty(ext))
                        ext = ".png";

                    var targetFileName = $"{_original.ID}{ext}";
                    var targetPath = Path.Combine(targetDir, targetFileName);

                    // создаём папку при необходимости
                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);

                    var srcFull = Path.GetFullPath(src);
                    var targetFull = Path.GetFullPath(targetPath);

                    if (!string.Equals(srcFull, targetFull, StringComparison.OrdinalIgnoreCase))
                    {
                        // Если файл уже есть — спрашиваем перезаписать
                        if (File.Exists(targetFull))
                        {
                            var res = MessageBox.Show(
                                $"Файл {targetFileName} уже существует в папке {folderName}.\nПерезаписать?",
                                "Перезапись файла", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (res == MessageBoxResult.Yes)
                                File.Copy(srcFull, targetFull, true);
                            else
                            {
                                // Если пользователь отказался перезаписывать, но в папке есть файл — используем существующий
                                if (!File.Exists(targetFull))
                                {
                                    // нет файла — не сохраняем путь
                                    _workingCopy.ImagePath = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            File.Copy(srcFull, targetFull, true);
                        }
                    }

                    // Записываем относительный путь (относительно каталога приложения),
                    // чтобы при сохранении в JSON было переносимо
                    _workingCopy.ImagePath = Path.Combine(folderName, targetFileName);
                }
                else
                {
                    // Если путь не абсолютный — считаем, что это относительный путь внутри приложения
                    // Проверим наличие файла: если нет — очищаем путь
                    var combined = Path.IsPathRooted(_workingCopy.ImagePath)
                        ? _workingCopy.ImagePath
                        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _workingCopy.ImagePath);
                    if (!File.Exists(combined))
                        _workingCopy.ImagePath = string.Empty;
                }
            }

            // копируем значения обратно в оригинал
            var type = _original.GetType();
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    var val = prop.GetValue(_workingCopy);
                    prop.SetValue(_original, val);
                }
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка при сохранении изображения: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Все файлы|*.*"
        };
        if (dlg.ShowDialog() == true)
        {
            _workingCopy.ImagePath = dlg.FileName;
        }
    }
}
