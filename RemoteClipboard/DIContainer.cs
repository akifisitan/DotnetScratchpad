using Microsoft.Extensions.DependencyInjection;

namespace RemoteClipboard;

internal static class DIContainer
{
    private static IServiceProvider? _serviceProvider = null;

    public static T GetRequiredService<T>()
        where T : notnull
    {
        if (_serviceProvider is null)
        {
            throw new Exception();
        }

        return _serviceProvider.GetRequiredService<T>();
    }

    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        if (_serviceProvider is not null)
        {
            throw new Exception();
        }

        _serviceProvider = serviceProvider;
    }

    public static void Dispose()
    {
        ((IDisposable?)_serviceProvider)?.Dispose();
    }
}
