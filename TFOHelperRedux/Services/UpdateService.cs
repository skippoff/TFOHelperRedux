using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace TFOHelperRedux.Services
{
    /// <summary>
    /// Сервис автообновления приложения без сторонних библиотек
    /// </summary>
    public class UpdateService
    {
        private readonly string _updateXmlUrl;
        private readonly string _appDirectory;
        private readonly string _currentVersion;
        private readonly string _batFilePath;
        private readonly string _tempZipPath;
        private readonly string _tempExtractPath;

        public UpdateService(string updateXmlUrl)
        {
            _updateXmlUrl = updateXmlUrl;
            _appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";
            _batFilePath = Path.Combine(_appDirectory, "update.bat");
            _tempZipPath = Path.Combine(Path.GetTempPath(), "TFOHelperRedux_update.zip");
            _tempExtractPath = Path.Combine(Path.GetTempPath(), "TFOHelperRedux_update_extracted");
        }

        /// <summary>
        /// Проверяет наличие обновлений и загружает их при наличии
        /// </summary>
        public async Task<bool> CheckAndUpdateAsync()
        {
            try
            {
                var latestVersionInfo = await DownloadUpdateInfoAsync();
                if (latestVersionInfo == null)
                {
                    Log("Не удалось получить информацию об обновлениях");
                    return false;
                }

                if (!IsUpdateAvailable(latestVersionInfo.Version))
                {
                    Log($"Текущая версия актуальна: {_currentVersion}");
                    return false;
                }

                Log($"Доступна новая версия: {latestVersionInfo.Version} (текущая: {_currentVersion})");

                // Показываем диалог пользователю
                var userConfirmed = ShowUpdateDialog(latestVersionInfo.Version);
                if (!userConfirmed)
                {
                    Log("Пользователь отменил обновление");
                    return false;
                }

                // Скачиваем с прогресс-баром
                var zipDownloaded = await DownloadUpdateWithProgressAsync(latestVersionInfo.ZipUrl);
                if (!zipDownloaded)
                {
                    Log("Ошибка загрузки обновления");
                    return false;
                }

                ExtractUpdate();

                CreateUpdateBatch(latestVersionInfo.ZipUrl);

                RestartWithUpdate();

                return true;
            }
            catch (Exception ex)
            {
                Log($"Ошибка обновления: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Показывает диалоговое окно с предложением обновиться
        /// </summary>
        private bool ShowUpdateDialog(string latestVersion)
        {
            var message = $"Доступна версия {latestVersion}.\n\n" +
                          $"Текущая версия: {_currentVersion}\n\n" +
                          $"Обновить сейчас?";

            var result = MessageBox.Show(
                message,
                "Доступно обновление",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes);

            return result == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Загружает XML файл с информацией об обновлении
        /// </summary>
        private async Task<UpdateInfo?> DownloadUpdateInfoAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var xmlContent = await httpClient.GetStringAsync(_updateXmlUrl);

                var doc = new XmlDocument();
                doc.LoadXml(xmlContent);

                var versionNode = doc.SelectSingleNode("//item/version");
                var urlNode = doc.SelectSingleNode("//item/url");

                if (versionNode == null || urlNode == null)
                {
                    Log("Неверный формат update.xml");
                    return null;
                }

                return new UpdateInfo
                {
                    Version = versionNode.InnerText,
                    ZipUrl = urlNode.InnerText
                };
            }
            catch (Exception ex)
            {
                Log($"Ошибка загрузки update.xml: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Сравнивает версии и определяет необходимость обновления
        /// </summary>
        private bool IsUpdateAvailable(string latestVersion)
        {
            try
            {
                var current = new Version(_currentVersion);
                var latest = new Version(latestVersion);
                return latest > current;
            }
            catch
            {
                // Если версии не удалось распарсить, сравниваем как строки
                return latestVersion != _currentVersion;
            }
        }

        /// <summary>
        /// Загружает ZIP архив с обновлением и показывает прогресс
        /// </summary>
        private async Task<bool> DownloadUpdateWithProgressAsync(string zipUrl)
        {
            var progressDialog = new DownloadProgressDialog();

            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(5);

                // Получаем размер файла
                var response = await httpClient.GetAsync(zipUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;

                // Запускаем загрузку с прогрессом
                var downloadTask = DownloadFileAsync(httpClient, zipUrl, totalBytes, progressDialog);

                // Показываем диалог
                progressDialog.Show();

                await downloadTask;

                progressDialog.Close();

                Log($"ZIP загружен: {_tempZipPath} ({totalBytes} байт)");
                return true;
            }
            catch (Exception ex)
            {
                progressDialog.Close();
                Log($"Ошибка загрузки ZIP: {ex.Message}");
                MessageBox.Show(
                    $"Не удалось загрузить обновление:\n{ex.Message}",
                    "Ошибка обновления",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Асинхронно загружает файл и обновляет прогресс
        /// </summary>
        private async Task DownloadFileAsync(HttpClient httpClient, string url, long totalBytes, DownloadProgressDialog progressDialog)
        {
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(_tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalBytesRead += bytesRead;

                if (totalBytes > 0)
                {
                    var progress = (int)(totalBytesRead * 100 / totalBytes);
                    progressDialog.UpdateProgress(progress, totalBytesRead, totalBytes);
                }
            }
        }

        /// <summary>
        /// Распаковывает ZIP архив во временную папку
        /// </summary>
        private void ExtractUpdate()
        {
            try
            {
                // Очищаем временную папку
                if (Directory.Exists(_tempExtractPath))
                    Directory.Delete(_tempExtractPath, true);

                Directory.CreateDirectory(_tempExtractPath);

                ZipFile.ExtractToDirectory(_tempZipPath, _tempExtractPath);

                Log($"Распаковано в: {_tempExtractPath}");
            }
            catch (Exception ex)
            {
                Log($"Ошибка распаковки: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Создаёт BAT-файл для замены файлов и перезапуска
        /// </summary>
        private void CreateUpdateBatch(string zipUrl)
        {
            var batContent = new StringBuilder();
            batContent.AppendLine("@echo off");
            batContent.AppendLine("chcp 65001 >nul");
            batContent.AppendLine();
            batContent.AppendLine("REM Скрипт обновления TFOHelperRedux");
            batContent.AppendLine("REM Ждёт закрытия приложения и заменяет файлы");
            batContent.AppendLine();
            batContent.AppendLine($"set APP_DIR={_appDirectory}");
            batContent.AppendLine($"set TEMP_ZIP={_tempZipPath}");
            batContent.AppendLine($"set TEMP_EXTRACT={_tempExtractPath}");
            batContent.AppendLine($"set EXE_NAME={Process.GetCurrentProcess().ProcessName}.exe");
            batContent.AppendLine();
            batContent.AppendLine("echo Ожидание закрытия приложения...");
            batContent.AppendLine();
            batContent.AppendLine(":wait_loop");
            batContent.AppendLine("tasklist /FI \"IMAGENAME eq %EXE_NAME%\" /NH | find /I \"%EXE_NAME%\" >nul");
            batContent.AppendLine("if not errorlevel 1 (");
            batContent.AppendLine("    timeout /t 1 /nobreak >nul");
            batContent.AppendLine("    goto wait_loop");
            batContent.AppendLine(")");
            batContent.AppendLine();
            batContent.AppendLine("echo Приложение закрыто. Начинаю обновление...");
            batContent.AppendLine();

            // === ЗАЩИТА пользовательских файлов ===
            
            // 1. CatchPoints_Local.json (точки лова)
            batContent.AppendLine("REM --- Защита CatchPoints_Local.json ---");
            batContent.AppendLine("set CATCHPOINTS_FILE=%APP_DIR%\\Maps\\CatchPoints_Local.json");
            batContent.AppendLine("set BACKUP_CATCHPOINTS=%TEMP%\\CatchPoints_backup_%RANDOM%.json");
            batContent.AppendLine("set CATCHPOINTS_SAVED=0");
            batContent.AppendLine("if exist \"%CATCHPOINTS_FILE%\" (");
            batContent.AppendLine("    copy /Y \"%CATCHPOINTS_FILE%\" \"%BACKUP_CATCHPOINTS%\" >nul");
            batContent.AppendLine("    if errorlevel 1 (");
            batContent.AppendLine("        echo ОШИБКА: не удалось сохранить точки лова!");
            batContent.AppendLine("        pause");
            batContent.AppendLine("        exit /b 1");
            batContent.AppendLine("    )");
            batContent.AppendLine("    set CATCHPOINTS_SAVED=1");
            batContent.AppendLine(")");
            batContent.AppendLine();

            // 2. BaitRecipes.json (рецепты прикормок)
            batContent.AppendLine("REM --- Защита BaitRecipes.json ---");
            batContent.AppendLine("set RECIPES_FILE=%APP_DIR%\\Recipes\\BaitRecipes.json");
            batContent.AppendLine("set BACKUP_RECIPES=%TEMP%\\BaitRecipes_backup_%RANDOM%.json");
            batContent.AppendLine("set RECIPES_SAVED=0");
            batContent.AppendLine("if exist \"%RECIPES_FILE%\" (");
            batContent.AppendLine("    copy /Y \"%RECIPES_FILE%\" \"%BACKUP_RECIPES%\" >nul");
            batContent.AppendLine("    if errorlevel 1 (");
            batContent.AppendLine("        echo ОШИБКА: не удалось сохранить рецепты!");
            batContent.AppendLine("        pause");
            batContent.AppendLine("        exit /b 1");
            batContent.AppendLine("    )");
            batContent.AppendLine("    set RECIPES_SAVED=1");
            batContent.AppendLine(")");
            batContent.AppendLine();

            // Копирование файлов обновления с проверкой
            batContent.AppendLine("xcopy /E /Y /I \"%TEMP_EXTRACT%\\*\" \"%APP_DIR%\"");
            batContent.AppendLine("if errorlevel 1 (");
            batContent.AppendLine("    echo ОШИБКА: не удалось скопировать файлы!");
            batContent.AppendLine("    if \"%CATCHPOINTS_SAVED%\"==\"1\" copy /Y \"%BACKUP_CATCHPOINTS%\" \"%CATCHPOINTS_FILE%\" >nul");
            batContent.AppendLine("    if \"%RECIPES_SAVED%\"==\"1\" copy /Y \"%BACKUP_RECIPES%\" \"%RECIPES_FILE%\" >nul");
            batContent.AppendLine("    pause");
            batContent.AppendLine("    exit /b 1");
            batContent.AppendLine(")");
            batContent.AppendLine();

            // Восстановление файлов после успешного копирования
            batContent.AppendLine("REM Восстановление пользовательских данных...");
            batContent.AppendLine("if \"%CATCHPOINTS_SAVED%\"==\"1\" (");
            batContent.AppendLine("    copy /Y \"%BACKUP_CATCHPOINTS%\" \"%CATCHPOINTS_FILE%\" >nul");
            batContent.AppendLine("    if errorlevel 1 (");
            batContent.AppendLine("        echo ОШИБКА: не удалось восстановить точки лова!");
            batContent.AppendLine("        echo Бэкап: %BACKUP_CATCHPOINTS%");
            batContent.AppendLine("        pause");
            batContent.AppendLine("        exit /b 1");
            batContent.AppendLine("    )");
            batContent.AppendLine("    del \"%BACKUP_CATCHPOINTS%\" 2>nul");
            batContent.AppendLine(")");
            batContent.AppendLine();
            batContent.AppendLine("if \"%RECIPES_SAVED%\"==\"1\" (");
            batContent.AppendLine("    copy /Y \"%BACKUP_RECIPES%\" \"%RECIPES_FILE%\" >nul");
            batContent.AppendLine("    if errorlevel 1 (");
            batContent.AppendLine("        echo ОШИБКА: не удалось восстановить рецепты!");
            batContent.AppendLine("        echo Бэкап: %BACKUP_RECIPES%");
            batContent.AppendLine("        pause");
            batContent.AppendLine("        exit /b 1");
            batContent.AppendLine("    )");
            batContent.AppendLine("    del \"%BACKUP_RECIPES%\" 2>nul");
            batContent.AppendLine(")");
            batContent.AppendLine();

            batContent.AppendLine("REM Очищаем временные файлы");
            batContent.AppendLine("del \"%TEMP_ZIP%\" 2>nul");
            batContent.AppendLine("rmdir /S /Q \"%TEMP_EXTRACT%\" 2>nul");
            batContent.AppendLine();
            batContent.AppendLine("echo Обновление завершено успешно!");
            batContent.AppendLine();
            batContent.AppendLine("REM Запускаем обновлённое приложение");
            batContent.AppendLine($"start \"\" \"%APP_DIR%\\%EXE_NAME%\"");
            batContent.AppendLine();
            batContent.AppendLine("REM Удаляем этот bat-файл");
            batContent.AppendLine("del \"%~f0\"");

            File.WriteAllText(_batFilePath, batContent.ToString(), Encoding.UTF8);
            Log($"BAT-файл создан: {_batFilePath}");
        }

        /// <summary>
        /// Запускает BAT-файл и закрывает текущее приложение
        /// </summary>
        private void RestartWithUpdate()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _batFilePath,
                    WorkingDirectory = _appDirectory,
                    CreateNoWindow = false,
                    UseShellExecute = true
                };

                Process.Start(startInfo);

                Log("BAT-файл запущен, закрытие приложения...");

                // Закрываем приложение
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Log($"Ошибка запуска BAT: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Логирование в debug output и файл
        /// </summary>
        private void Log(string message)
        {
            var logMessage = $"[UpdateService] {DateTime.Now:HH:mm:ss.fff} {message}";
            Debug.WriteLine(logMessage);

            // Пытаемся записать в лог-файл если существует
            try
            {
                var logPath = Path.Combine(_appDirectory, "logs", $"app-{DateTime.Now:yyyy-MM-dd}.log");
                var logDir = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);
                
                File.AppendAllText(logPath, logMessage + Environment.NewLine);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
        }

        /// <summary>
        /// Информация об обновлении из XML
        /// </summary>
        private class UpdateInfo
        {
            public string Version { get; set; } = string.Empty;
            public string ZipUrl { get; set; } = string.Empty;
        }
    }
}
