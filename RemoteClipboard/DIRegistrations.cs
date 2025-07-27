using Microsoft.Extensions.DependencyInjection;
using RemoteClipboard.Abstractions;
using RemoteClipboard.Services;
using Scratchpad.Lib.Clipboard;

namespace RemoteClipboard;

internal static class DIRegistrations
{
    public static IServiceCollection AddRemoteClipboard(this IServiceCollection services)
    {
        //services.AddSingleton<IClipboardService, RemoteClipboardService>();
        services.AddSingleton<IClipboardService, LocalTextFileClipboardService>();
        services.AddSingleton<IAuthService, DummyAuthService>();
        services.AddSingleton<IAppService, AppService>();
        services.AddSingleton<ISecureStorage, SecureStorage>();

        return services;
    }
}
