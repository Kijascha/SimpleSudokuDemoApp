using Microsoft.Extensions.DependencyInjection;
using SimpleSudokuDemo.Core;

namespace SimpleSudokuDemo.Services;

public static class ServiceExtensions
{
    public static void AddFactory<T>(this IServiceCollection services)
        where T : class
    {
        services.AddTransient<T>();
        services.AddSingleton<Func<T>>((p) => () => p.GetService<T>()!);
        services.AddSingleton<IAbstractFactory<T>, AbstractFactory<T>>();
    }

    public static IAbstractFactory<T> GetAbstractFactory<T>(this IServiceProvider provider)
    {
        return provider.GetRequiredService<IAbstractFactory<T>>();
    }
    public static void AddNavigationService(this IServiceCollection services)
    {
        services.AddSingleton<INavigationService, NavigationService>();

    }
    public static void AddGameService(this IServiceCollection services)
    {
        services.AddSingleton<IGameService, GameService>();

    }
}
