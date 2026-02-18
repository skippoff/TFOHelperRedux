using Microsoft.Extensions.DependencyInjection;
using TFOHelperRedux.Services.Business;
using TFOHelperRedux.Services.Data;
using TFOHelperRedux.Services.UI;
using TFOHelperRedux.Services.State;
using TFOHelperRedux.ViewModels;

namespace TFOHelperRedux.Services.DI;

/// <summary>
/// Контейнер сервисов приложения
/// </summary>
public static class ServiceContainer
{
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    /// Инициализация контейнера сервисов
    /// </summary>
    public static void Initialize()
    {
        var services = new ServiceCollection();

        // Регистрируем сервисы
        RegisterServices(services);

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Получение сервиса из контейнера
    /// </summary>
    public static T GetService<T>() where T : notnull
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("ServiceContainer не инициализирован. Вызовите Initialize() первым.");

        var service = _serviceProvider.GetService<T>();
        if (service == null)
            throw new InvalidOperationException($"Сервис типа {typeof(T).Name} не зарегистрирован.");

        return service;
    }

    /// <summary>
    /// Получение сервиса из контейнера (не-generic)
    /// </summary>
    public static object? GetService(System.Type serviceType)
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("ServiceContainer не инициализирован. Вызовите Initialize() первым.");

        return _serviceProvider.GetService(serviceType);
    }

    private static void RegisterServices(ServiceCollection services)
    {
        // UI сервис
        services.AddSingleton<IUIService, UIService>();

        // Сервис загрузки/сохранения данных
        services.AddSingleton<IDataLoadSaveService, DataLoadSaveService>();

        // Debouncer для отложенного сохранения
        services.AddSingleton<SaveDebouncer>();

        // Сервисы
        services.AddSingleton<FishDataService>(provider =>
        {
            var loadSaveService = provider.GetRequiredService<IDataLoadSaveService>();
            return new FishDataService(loadSaveService);
        });
        services.AddSingleton<BaitCrudService>(provider =>
        {
            var fishDataService = provider.GetRequiredService<FishDataService>();
            return new BaitCrudService(fishDataService);
        });
        services.AddSingleton<LureBindingService>(provider =>
        {
            var loadSaveService = provider.GetRequiredService<IDataLoadSaveService>();
            return new LureBindingService(loadSaveService);
        });
        services.AddSingleton<FishFilterService>();
        services.AddSingleton<CatchPointsService>(provider =>
        {
            var uiService = provider.GetRequiredService<IUIService>();
            var loadSaveService = provider.GetRequiredService<IDataLoadSaveService>();
            return new CatchPointsService(uiService, loadSaveService);
        });

        // MapsService требует фабрику из-за параметров Action
        services.AddTransient<MapsService>(provider =>
        {
            var loadSaveService = provider.GetRequiredService<IDataLoadSaveService>();
            return new MapsService(
                loadSaveService,
                DataStore.Maps,
                onMapsChanged: null,
                onSelectedMapChanged: null,
                onSelectedLevelFilterChanged: null
            );
        });

        // ViewModels
        services.AddTransient<NavigationViewModel>();
        services.AddTransient<BaitRecipesViewModel>(provider =>
        {
            var uiService = provider.GetRequiredService<IUIService>();
            return new BaitRecipesViewModel(uiService);
        });
        services.AddTransient<CatchPointsViewModel>(provider =>
        {
            var catchPointsService = provider.GetRequiredService<CatchPointsService>();
            var uiService = provider.GetRequiredService<IUIService>();
            return new CatchPointsViewModel(catchPointsService, uiService);
        });
        services.AddTransient<BaitsViewModel>(provider =>
        {
            var baitCrudService = provider.GetRequiredService<BaitCrudService>();
            var uiService = provider.GetRequiredService<IUIService>();
            return new BaitsViewModel(baitCrudService, uiService);
        });
        services.AddTransient<FishViewModel>(provider =>
        {
            var filterService = provider.GetRequiredService<FishFilterService>();
            var lureBindingService = provider.GetRequiredService<LureBindingService>();
            var fishDataService = provider.GetRequiredService<FishDataService>();
            var mapsService = provider.GetRequiredService<MapsService>();
            var navigationVM = provider.GetRequiredService<NavigationViewModel>();
            var baitsVM = provider.GetRequiredService<BaitsViewModel>();
            var baitRecipesVM = provider.GetRequiredService<BaitRecipesViewModel>();
            var catchPointsVM = provider.GetRequiredService<CatchPointsViewModel>();
            
            return new FishViewModel(
                filterService,
                lureBindingService,
                fishDataService,
                mapsService,
                navigationVM,
                baitsVM,
                baitRecipesVM,
                catchPointsVM);
        });
    }
}