using Microsoft.Extensions.DependencyInjection;
using Scratchpad.ConsoleApp;
using Scratchpad.Lib;

using var serviceProvider = new ServiceCollection()
    .AddScratchPadLib()
    .AddSingleton<IRunner, Runner>()
    .AddSingleton<LogSearchRunner>()
    .BuildServiceProvider();

var service = serviceProvider.GetRequiredService<IRunner>();

await service.Run(args);
