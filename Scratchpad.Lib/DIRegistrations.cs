using Microsoft.Extensions.DependencyInjection;
using Scratchpad.Lib.Abstractions;

namespace Scratchpad.Lib;

public static class DIRegistrations
{
    public static IServiceCollection AddScratchPadLib(this IServiceCollection services)
    {
        services.AddSingleton<ISecureStorage, DpapiSecureStorage>();
        services.AddSingleton<IDirectoryPacker, DirectoryPacker>();

        return services;
    }
}
