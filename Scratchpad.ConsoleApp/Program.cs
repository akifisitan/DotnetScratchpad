using Microsoft.Extensions.DependencyInjection;
using Scratchpad.ConsoleApp;
using Scratchpad.Lib;

var serviceCollection = new ServiceCollection();

serviceCollection.AddScratchPadLib();
serviceCollection.AddSingleton<IRunner, Runner>();

using var serviceProvider = serviceCollection.BuildServiceProvider();

var service = serviceProvider.GetRequiredService<IRunner>();

await service.Run(args);
