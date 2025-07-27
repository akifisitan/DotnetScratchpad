using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RemoteClipboard.Model;

namespace RemoteClipboard;

public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var services = new ServiceCollection();

        services.AddRemoteClipboard();

        var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        DIContainer.SetServiceProvider(serviceProvider);
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        DIContainer.Dispose();

        DesktopContext.HotKeyManager.Dispose();
    }
}
