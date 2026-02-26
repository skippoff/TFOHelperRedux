using Microsoft.Extensions.DependencyInjection;
using Serilog;
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
    private static readonly ILogger _log = Log.ForContext(typeof(ServiceContainer));
    
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    /// Инициализация контейнера сервисов
    /// </summary>
    public static void Initialize()
    {
        _log.Information("Инициализация ServiceContainer...");
        
        var services = new ServiceCollection();

        // Регистрируем сервисы
        _log.Debug("Регистрация сервисов...");
        RegisterServices(services);

        _serviceProvider = services.BuildServiceProvider();
        
        _log.Information("ServiceContainer инициализирован");
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
        {
            _log.Warning("Сервис {Type} не найден", typeof(T).Name);
            throw new InvalidOperationException($"Сервис типа {typeof(T).Name} не зарегистрирован.");
        }

        _log.Verbose("Получен сервис {Type}", typeof(T).Name);
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
        // UI сервисы
        services.AddSingleton<IUIService, UIService>();
        services.AddSingleton<ThemeService>();
        _log.Verbose("Зарегистрирован IUIService и ThemeService");

        // Сервис настроек пользователя
        services.AddSingleton<SettingsService>();
        _log.Verbose("Зарегистрирован SettingsService");

        // Сервис загрузки/сохранения данных
        services.AddSingleton<IDataLoadSaveService, DataLoadSaveService>();
        _log.Verbose("Зарегистрирован IDataLoadSaveService");

        // Debouncer для отложенного сохранения
        services.AddSingleton<SaveDebouncer>();

        // Сервисы
        services.AddSingleton<FishDataService>(provider =>
        {
            var loadSaveService = provider.GetRequiredService<IDataLoadSaveService>();
            return new FishDataService(loadSaveService);
        });
        _log.Verbose("Зарегистрирован FishDataService");
        
        services.AddSingleton<BaitCrudService>(provider =>
        {
            var fishDataService = provider.GetRequiredService<FishDataService>();
            return new BaitCrudService(fishDataService);
        });
        _log.Verbose("Зарегистрирован BaitCrudService");

        services.AddSingleton<FishFilterService>();
        _log.Verbose("Зарегистрирован FishFilterService");
        
        services.AddSingleton<CatchPointsService>(provider =>
        {
            var uiService = provider.GetRequiredService<IUIService>();
            var loadSaveService = provider.GetRequiredService<IDataLoadSaveService>();
            return new CatchPointsService(uiService, loadSaveService);
        });
        _log.Verbose("Зарегистрирован CatchPointsService");

        services.AddSingleton<MapListViewService>();
        _log.Verbose("Зарегистрирован MapListViewService");

        services.AddSingleton<FishFeedsViewModel>();
        _log.Verbose("Зарегистрирован FishFeedsViewModel");

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
        _log.Verbose("Зарегистрирован MapsService");

        // ViewModels
        services.AddTransient<NavigationViewModel>();
        _log.Verbose("Зарегистрирован NavigationViewModel");
        
        services.AddTransient<BaitRecipesViewModel>(provider =>
        {
            var uiService = provider.GetRequiredService<IUIService>();
            return new BaitRecipesViewModel(uiService);
        });
        _log.Verbose("Зарегистрирован BaitRecipesViewModel");
        
        services.AddTransient<CatchPointsViewModel>(provider =>
        {
            var catchPointsService = provider.GetRequiredService<CatchPointsService>();
            var uiService = provider.GetRequiredService<IUIService>();
            return new CatchPointsViewModel(catchPointsService, uiService);
        });
        _log.Verbose("Зарегистрирован CatchPointsViewModel");
        
        services.AddTransient<BaitsViewModel>(provider =>
        {
            var baitCrudService = provider.GetRequiredService<BaitCrudService>();
            var uiService = provider.GetRequiredService<IUIService>();
            return new BaitsViewModel(baitCrudService, uiService);
        });
        _log.Verbose("Зарегистрирован BaitsViewModel");
        
        services.AddTransient<FishViewModel>(provider =>
        {
            var filterService = provider.GetRequiredService<FishFilterService>();
            var fishDataService = provider.GetRequiredService<FishDataService>();
            var mapsService = provider.GetRequiredService<MapsService>();
            var mapListViewService = provider.GetRequiredService<MapListViewService>();
            var navigationVM = provider.GetRequiredService<NavigationViewModel>();
            var baitsVM = provider.GetRequiredService<BaitsViewModel>();
            var baitRecipesVM = provider.GetRequiredService<BaitRecipesViewModel>();
            var catchPointsVM = provider.GetRequiredService<CatchPointsViewModel>();
            var fishFeedsVM = provider.GetRequiredService<FishFeedsViewModel>();

            return new FishViewModel(
                filterService,
                fishDataService,
                mapsService,
                mapListViewService,
                navigationVM,
                baitsVM,
                baitRecipesVM,
                catchPointsVM,
                fishFeedsVM);
        });
        _log.Verbose("Зарегистрирован FishViewModel");
    }
}