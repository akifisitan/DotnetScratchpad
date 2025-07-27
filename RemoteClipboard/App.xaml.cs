using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace RemoteClipboard;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddRemoteClipboard();

        var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        DIContainer.SetServiceProvider(serviceProvider);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        DIContainer.Dispose();
    }
}
