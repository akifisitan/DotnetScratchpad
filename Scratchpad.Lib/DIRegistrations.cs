using Microsoft.Extensions.DependencyInjection;
using Scratchpad.Lib.Abstractions;

namespace Scratchpad.Lib;

public static class DIRegistrations
{
    public static void AddScratchPadLib(this ServiceCollection services)
    {
        services.AddSingleton<ISecureStorage, DpapiSecureStorage>();
        services.AddSingleton<IDirectoryPacker, DirectoryPacker>();
        //services.AddSingleton<IClipboardService, RemoteClipboardService>();
        services.AddSingleton<IClipboardService, LocalTextFileClipboardService>();
    }
}
