using Microsoft.Extensions.DependencyInjection;

namespace TFOHelperRedux.Services;

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
        services.AddTransient<ViewModels.NavigationViewModel>();
        services.AddTransient<ViewModels.BaitRecipesViewModel>(provider =>
        {
            var uiService = provider.GetRequiredService<IUIService>();
            return new ViewModels.BaitRecipesViewModel(uiService);
        });
        services.AddTransient<ViewModels.CatchPointsViewModel>(provider =>
        {
            var catchPointsService = provider.GetRequiredService<CatchPointsService>();
            var uiService = provider.GetRequiredService<IUIService>();
            return new ViewModels.CatchPointsViewModel(catchPointsService, uiService);
        });
        services.AddTransient<ViewModels.BaitsViewModel>(provider =>
        {
            var baitCrudService = provider.GetRequiredService<BaitCrudService>();
            var uiService = provider.GetRequiredService<IUIService>();
            return new ViewModels.BaitsViewModel(baitCrudService, uiService);
        });
        services.AddTransient<ViewModels.FishViewModel>(provider =>
        {
            var filterService = provider.GetRequiredService<FishFilterService>();
            var lureBindingService = provider.GetRequiredService<LureBindingService>();
            var fishDataService = provider.GetRequiredService<FishDataService>();
            var mapsService = provider.GetRequiredService<MapsService>();
            var navigationVM = provider.GetRequiredService<ViewModels.NavigationViewModel>();
            var baitsVM = provider.GetRequiredService<ViewModels.BaitsViewModel>();
            var baitRecipesVM = provider.GetRequiredService<ViewModels.BaitRecipesViewModel>();
            var catchPointsVM = provider.GetRequiredService<ViewModels.CatchPointsViewModel>();
            
            return new ViewModels.FishViewModel(
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