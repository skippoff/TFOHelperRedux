using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TFOHelperRedux.Services;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux
{
    public partial class App : Application
    {
        public static new App Current => (App)Application.Current;
        
        public FishViewModel MainViewModel { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Инициализация контейнера сервисов
                ServiceContainer.Initialize();

                // Загрузка данных через DataStore (статический класс)
                DataStore.LoadAll();

                // Создание главной ViewModel через DI
                MainViewModel = ServiceContainer.GetService<ViewModels.FishViewModel>();

                // Создание и показ главного окна
                var window = new Views.FishWindow
                {
                    DataContext = MainViewModel
                };
                window.Show();
            }
            catch (Exception ex)
            {
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
            base.OnExit(e);

            try
            {
                DataStore.SaveAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении данных:\n" + ex.Message,
                                "TFO Helper 3.0 Redux (RU)",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
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
