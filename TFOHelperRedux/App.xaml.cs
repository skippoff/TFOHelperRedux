using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Serilog;
using Serilog.Events;
using TFOHelperRedux.Services.DI;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux
{
    public partial class App : Application
    {
        public static new App Current => (App)Application.Current;

        public FishViewModel MainViewModel { get; private set; } = null!;

        /// <summary>
        /// Глобальный логгер Serilog
        /// </summary>
        public static ILogger Log { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Настройка Serilog
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app-.log");

            Log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Debug(
                    outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    shared: true)
                .CreateLogger();

            Log.Information("=== Запуск приложения TFOHelperRedux ===");
            Log.Information("Версия .NET: {RuntimeVersion}", Environment.Version);
            Log.Information("Путь к приложению: {AppPath}", AppDomain.CurrentDomain.BaseDirectory);

            base.OnStartup(e);

            try
            {
                Log.Information("Инициализация ServiceContainer...");
                // Инициализация контейнера сервисов
                ServiceContainer.Initialize();

                Log.Information("Загрузка данных через DataStore...");
                // Загрузка данных через DataStore (статический класс)
                DataStore.LoadAll();

                Log.Information("Создание MainViewModel через DI...");
                // Создание главной ViewModel через DI
                MainViewModel = ServiceContainer.GetService<ViewModels.FishViewModel>();

                Log.Information("Создание и показ главного окна...");
                // Создание и показ главного окна
                var window = new Views.FishWindow
                {
                    DataContext = MainViewModel
                };
                window.Show();

                Log.Information("Приложение успешно запущено");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Критическая ошибка при запуске приложения");

                string message =
                    $"Ошибка при запуске приложения:\n\n" +
                    $"{ex.GetType().FullName}\n{ex.Message}\n\n" +
                    $"{(ex.InnerException != null ? "Внутренняя ошибка:\n" + ex.InnerException.Message + "\n\n" : "")}" +
                    $"Стек вызовов:\n{ex.StackTrace}";

                MessageBox.Show(message, "TFO Helper 3.0 Redux (RU)",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Выход из приложения...");
            
            base.OnExit(e);

            try
            {
                Log.Information("Сохранение данных...");
                DataStore.SaveAll();
                Log.Information("Данные сохранены");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при сохранении данных");
                MessageBox.Show("Ошибка при сохранении данных:\n" + ex.Message,
                                "TFO Helper 3.0 Redux (RU)",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                // Закрытие и очистка логгера
                Log.Information("=== Приложение закрыто ===");
                Serilog.Log.CloseAndFlush();
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scroll)
            {
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset - e.Delta / 3.0);
                e.Handled = true;
            }
        }
    }
}
